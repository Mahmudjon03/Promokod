using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PromoRandom.Models;
using PromoRandom.Services;

namespace PromoRandom.Controllers
{
    [Authorize]
    public class AdminController(DatabaseService databaseService, WinnerNotificationService winnerNotifier) : Controller
    {
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var priz = await databaseService.GetAllPrizesAsync();

            //var viewModel = new PrizeIndexViewModel
            //{
            //    Prizes = priz,
            //   // PrizeWithPromoAndUsers = prizpromuser
            //};
            return View(priz);
        }

        [HttpGet]
        public async Task<JsonResult> GetPromoCode(string prizeName)
        {
            var random = new Random();
            var activeGive = await databaseService.GetFirstIncompleteGiveAsync();

            if (activeGive == null)
            {
                return Json(new { error = "Нет активных розыгрышей" });
            }

            var promoCodes = await databaseService.GetPromoCodesByGiveIdAsync(activeGive.Id);
            var randomPromocode = promoCodes[random.Next(promoCodes.Count)];

            var user = await databaseService.GetUserByChatIdAsync(randomPromocode.UserChatId);

            if (user != null)
            {
                _ = Task.Run(() => NotifyUserAsync(prizeName, user)); // В фоне
            }

            return Json(new { promoCode = randomPromocode.Code, userName = user.Name });
        }

        private async Task NotifyUserAsync(string prizeName, User user)
        {
            await Task.Delay(30000); // Подождать 30 сек
            await winnerNotifier.SendWinnerMessageAsync(user.ChatId, user.Language, prizeName);
        }

        [HttpGet]
        public async Task<IActionResult> Setting()
        {
            var prize = await databaseService.GetAllPrizesAsync();

            return View(prize);
        }

        [HttpPost]
        public async Task<IActionResult> AddPrize(Prize model)
        {
            if (model == null)

                return RedirectToAction("Setting");

            await databaseService.AddPrizeAsync(model);

            return RedirectToAction("Setting");
        }

        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            await databaseService.DeletePrizeAsync(id);

            return RedirectToAction("Setting");
        }
    }
}
