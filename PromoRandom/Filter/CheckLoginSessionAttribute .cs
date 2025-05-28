using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Authentication;

public class CheckLoginSessionAttribute : Attribute, IAsyncAuthorizationFilter
{
    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        var httpContext = context.HttpContext;
        var session = httpContext.Session;

        // Проверка: если сессия login не установлена или равна 0
        int? login = session.GetInt32("login");
        if (login == null || login == 0)
        {
            // Удаляем куки (разлогиниваем)
            await httpContext.SignOutAsync("MyCookieAuth");

            // Перенаправляем на страницу логина
            context.Result = new RedirectToActionResult("Login", "Account", null);
        }
    }
}
