using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PromoRandom.Models;
using PromoRandom.Services;
using PromoRandom.ViewModel;
using System.Data;

namespace PromoRandom.Controllers
{
    [CheckLoginSession]
    public class HomeController(DatabaseService databaseService, WinnerNotificationService winnerNotifier) : Controller
    {
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Index(DateTime date)
        {

            HttpContext.Session.SetString("SelectedDate", date.ToString("O"));

            var priz = await databaseService.GetPrizes();


            var viewModel = new PrizeIndexViewModel
            {
                Prizes = priz,
                PrizeWithPromoAndUsers = prizpromuser
            };
            return View(viewModel);
        }

        [HttpGet]
        public async Task<JsonResult> GetPromoCode(string prizeName)
        {
            string? dateString = HttpContext.Session.GetString("SelectedDate");
            var random = new Random();
            var promoCodes = await databaseService.GetPromoCodesByUserAsync(dateString);
            string code = promoCodes[random.Next(promoCodes.Count)];
            var user = await databaseService.GetUserByPromokodAsync(code);

            if (user != null)
            {
                _ = Task.Run(() => NotifyUserAsync(prizeName, user)); // В фоне
            }

            return Json(new { promoCode = code, userName = user.Name });
        }

        private async Task NotifyUserAsync(string prizeName, User user)
        {
            await Task.Delay(30000); // Подождать 30 сек
            await winnerNotifier.SendWinnerMessageAsync(user.ChatId, user.Language, prizeName);
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Setting()
        {
            var prize = await databaseService.GetPrizes();

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
        public async Task<IActionResult> SavePrizeResult([FromBody] AddPrizeUserModel model)
        {
            await databaseService.UpdatePrizeAsync(model);
            return Ok();
        }
        
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
           await databaseService.DeletePrizeAsync(id);
           
            return RedirectToAction("Setting"); 
        }
    }
}
