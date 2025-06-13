using Microsoft.AspNetCore.Mvc;
using PromoRandom.Services;

namespace PromoRandom.Controllers
{
    public class SiteController(DatabaseService databaseService) : Controller
    {
        private readonly DatabaseService _databaseService = databaseService;

        public IActionResult Index()
        {
            ViewBag.EndDate = new DateTime(2025, 6, 15, 23, 59, 59, DateTimeKind.Utc); // пример даты окончания
            return View();
        }

        public async Task<IActionResult> Statistics()
        {
            var stats = await _databaseService.GetWinnerStatisticsAsync();
            return View(stats);
        }
    }

}
