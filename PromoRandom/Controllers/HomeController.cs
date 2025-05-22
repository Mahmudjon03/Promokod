using Microsoft.AspNetCore.Mvc;
using PromoRandom.Services;


namespace PromoRandom.Controllers
{
    public class HomeController(WinnerNotificationService winnerNotifier) : Controller
    {

        private readonly DatabaseService _databaseService = new();
        private readonly WinnerNotificationService _winnerNotifier = winnerNotifier;

        public IActionResult Index()
        {
            return View();
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
                await _winnerNotifier.SendWinnerMessageAsync(user.ChatId, user.Language, "IPhone 16 Pro Max");
            }

            return Json(new { promoCode = code, prize = $"{user.Name} 🎁 IPhone 16 Pro Max 😍!" });
        }

        [HttpGet]
        public IActionResult Setting()
        {
            return View();
        }
    }
}
