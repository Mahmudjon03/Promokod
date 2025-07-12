using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using PromoRandom.Models;
using System.Security.Claims;

namespace PromoRandom.Controllers;

public class AccountController(IOptionsMonitor<AdminUserOptions> adminOptions) : Controller
{

    [HttpGet]
    public IActionResult Index() => View();

    [HttpPost]
    public async Task<IActionResult> Login(string username, string password)
    {
        // Получаем актуальные данные администратора из конфигурации
        var adminUser = adminOptions.CurrentValue;

        if (username.Equals(adminUser.Username, StringComparison.OrdinalIgnoreCase) &&
            password == adminUser.Password)
        {
            // Формирование списка claims, используя стандартные типы (Role и Name)
            var claims = new List<Claim>
            {
                new(ClaimTypes.Name, adminUser.Username),
                new(ClaimTypes.Role, adminUser.Role)
            };

            // Создаём identity с использованием стандартной схемы cookie-аутентификации
            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);

            var authProperties = new AuthenticationProperties
            {
                ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(30)
            };

            // Подписываем пользователя: создаётся cookie, содержащая claims
            await HttpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            return RedirectToAction("Index", "Admin");
        }

        ModelState.AddModelError(string.Empty, "Неверный логин или пароль");
        return View("Index");
    }

    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        return RedirectToAction("Index");
    }
}
