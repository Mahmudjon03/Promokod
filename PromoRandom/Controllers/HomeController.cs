using Microsoft.AspNetCore.Mvc;
using PromoRandom.Models;
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

            string user = await _databaseService.GetUserByPromokod(code);

            return Json(new { promoCode = code, prize = $"{user} 🎁 priz😍!" });

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
        public IActionResult AddPrizUser(AddPrizUserModel model)
        {

            return RedirectToAction("Index");
        }

        [HttpPost]
        public async Task<IActionResult> SavePrizeResult([FromBody]AddPrizUserModel model)
        {
            
           await _databaseService.UpdatePrizeAsync(model);

             return Ok();
        }
    }
}
