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
        public IActionResult Index()
        {
            var prizeList = HttpContext.Session.GetObjectFromJson<List<Priz>>("PrizeList") ?? new List<Priz>();
            var getPriz = prizeList.Where(z => z.PrizState = false).ToList();


            return View(getPriz.FirstOrDefault());
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
            var prizeList = HttpContext.Session.GetObjectFromJson<List<Priz>>("PrizeList") ?? new List<Priz>();
            return View(prizeList);
        }

        
        [HttpPost]
        public IActionResult AddPrize(string prizName)
        {
            if (string.IsNullOrWhiteSpace(prizName))
                return RedirectToAction("Setting");

            var prizeList = HttpContext.Session.GetObjectFromJson<List<Priz>>("PrizeList") ?? new List<Priz>();
            prizeList.Add(new Priz { PrizName = prizName });

            HttpContext.Session.SetObjectAsJson("PrizeList", prizeList);

            return RedirectToAction("Setting");
        }
    }
}
