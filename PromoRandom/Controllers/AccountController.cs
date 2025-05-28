using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace PromoRandom.Controllers
{
    public class AccountController : Controller
    {
        [HttpGet]
        public IActionResult Login() => View();

        // Обработка логина
        [HttpPost]
        public async Task<IActionResult> Login(string username, string password)
        {
            
            if (username == "admin" && password == "1234")
            {
                var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Name, username),
                new Claim("MyAppRole", "Admin")
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
