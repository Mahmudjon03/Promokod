using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PromoRandom.Models;
using PromoRandom.Services;
using System.Data;

namespace PromoRandom.Controllers
{
    [CheckLoginSession]
    public class HomeController(WinnerNotificationService winnerNotifier) : Controller
    {
        private readonly DatabaseService _databaseService = new();
        private readonly WinnerNotificationService _winnerNotifier = winnerNotifier;

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Index(DateTime date)
        {

            HttpContext.Session.SetString("SelectedDate", date.ToString("O"));


            var priz = await _databaseService.GetPrizes();

            return View(priz);
        }

        [HttpGet]
        public async Task<JsonResult> GetPromoCode(string prizeName)
        {
            string? dateString = HttpContext.Session.GetString("SelectedDate");
            var random = new Random();
            var promoCodes = await _databaseService.GetPromoCodesByUserAsync(dateString);
            var code = promoCodes[random.Next(promoCodes.Count)];
            var user = await _databaseService.GetUserByPromokodAsync(code);

            /* string code = "4544B374"; //*/
            if (user != null)
            {
                _ = Task.Run(() => NotifyUserAsync(prizeName, user)); // В фоне
                return Json(new { promoCode = code, userName = user.Name });
            }

            return Json(new { error = "User not found" });
        }

        private async Task NotifyUserAsync(string prizeName, User user)
        {
            await Task.Delay(30000); // Подождать 30 сек
            await _winnerNotifier.SendWinnerMessageAsync(user.ChatId, user.Language, prizeName);
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Setting()
        {
            var prize = await _databaseService.GetPrizes();

            return View(prize);
        }

        [HttpPost]
        public async Task<IActionResult> AddPrize(Prize model)
        {
            await _databaseService.AddPrizeAsync(model);

            return RedirectToAction("Setting");
        }

        [HttpPost]
        public async Task<IActionResult> SavePrizeResult([FromBody] AddPrizeUserModel model)
        {
            await _databaseService.UpdatePrizeAsync(model);
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            await _databaseService.DeletePrizeAsync(id);


            return RedirectToAction("Setting");
        }
    }
}
