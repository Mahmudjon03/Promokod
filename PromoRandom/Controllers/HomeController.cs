using Microsoft.AspNetCore.Mvc;
using PromoRandom.Models;
using PromoRandom.Services;

namespace PromoRandom.Controllers
{
    public class HomeController(WinnerNotificationService winnerNotifier) : Controller
    {
        private readonly DatabaseService _databaseService = new();

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var priz = await _databaseService.GetPrizes();

            return View(priz);
        }

        [HttpGet]
        public async Task<JsonResult> GetPromoCode()
        {
            var random = new Random();
            var promoCodes = await _databaseService.GetPromoCodesByUserAsync();
            string code = promoCodes[random.Next(promoCodes.Count)];

            var user = await _databaseService.GetUserByPromokodAsync(code);

            if (user != null)
            {
                await winnerNotifier.SendWinnerMessageAsync(user.ChatId, user.Language, "IPhone 16 Pro Max");
            }

            return Json(new { promoCode = code, prize = $"{user} 🎁 prize 😍!" });
        }

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

        [HttpPost]
        public IActionResult AddPrizeUser(AddPrizeUserModel model)
        {
            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> SavePrizeResult([FromBody] AddPrizeUserModel model)
        {
            await _databaseService.UpdatePrizeAsync(model);

            return Ok();
        }
    }
}
