using Microsoft.AspNetCore.Mvc;
using PromoRandom.Services;


namespace PromoRandom.Controllers
{
    public class HomeController : Controller
    {

        private readonly DatabaseService _databaseService;

        public HomeController()
        {
            _databaseService = new DatabaseService();
        }
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

            string user = await _databaseService.GetUserByPromokod(code);


            return Json(new { promoCode = code, prize = $"{user} 🎁 IPhone 16 Pro Max 😍!" });

        }

        [HttpGet]
        public IActionResult Setting()
        {
            return View();  
        }
    }
}
