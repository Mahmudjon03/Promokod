using Microsoft.AspNetCore.Mvc;

namespace PromoRandom.Controllers
{
    public class SiteController : Controller
    {
        public IActionResult Index()
        {
            ViewBag.EndDate = new DateTime(2025, 6, 15, 23, 59, 59, DateTimeKind.Utc); // пример даты окончания
            return View();
        }

    }
}
