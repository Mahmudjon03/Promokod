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
            string code = "G7A9KZ3X"; // promoCodes[random.Next(promoCodes.Count)];
            var user = await _databaseService.GetUserByPromokodAsync(code);

            if (user != null)
            {
                _ = Task.Run(() => NotifyUserAsync(prizeName, user)); // В фоне
            }

<<<<<<< HEAD
            return Json(new { promoCode = code, prize = $"{user} 🎁 prize 😍!" });
=======

            return Json(new { promoCode = code, userName = user.Name });
>>>>>>> origin/main
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
            if (model == null)
               
                return RedirectToAction("Setting");

                await _databaseService.AddPrizeAsync(model);

            return RedirectToAction("Setting");
        }

<<<<<<< HEAD
        [HttpPost]
        public IActionResult AddPrizeUser(AddPrizeUserModel model)
        {
            return RedirectToAction("Index");
        }
=======



>>>>>>> origin/main

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
