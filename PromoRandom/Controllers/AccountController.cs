using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using PromoRandom.Services;
using System.Security.Claims;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace PromoRandom.Controllers
{
    public class AccountController() : Controller
    {
        private readonly DatabaseService _databaseService = new();


        [HttpGet]
        public IActionResult Login() => View();

        // Обработка логина
        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            var customers = await _databaseService.GetCustomers();
            var customer = customers.FirstOrDefault(c =>
                c.Name.Equals(username, StringComparison.OrdinalIgnoreCase) &&
                c.Password == password);

            if (customer != null)
            {
                var claims = new List<Claim>
        {
            new Claim(ClaimTypes.Name, customer.Name),
            new Claim("MyAppRole", customer.Role)
        };

                var claimsIdentity = new ClaimsIdentity(claims, "MyCookieAuth");

                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = false
                };

                await HttpContext.SignInAsync("MyCookieAuth",
                    new ClaimsPrincipal(claimsIdentity), authProperties);

                HttpContext.Session.SetInt32("login", 1);
                return RedirectToAction("Setting", "Home");
            }

            ViewBag.Error = "Неверный логин или пароль";
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("MyCookieAuth");
            return RedirectToAction("Login");
        }


    }
}
