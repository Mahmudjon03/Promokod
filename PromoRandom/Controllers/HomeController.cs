using AksiyaBot;
using Microsoft.AspNetCore.Mvc;


namespace YourProject.Controllers
{
    public class HomeController : Controller
    {
        private static readonly List<string> PromoCodes = new List<string>
        {
            "G7A9KZ3X", "M2T6CV8W", "X0J7WZ9L", "B9K1LQ2M", "Z8N4HR5C",
            "W3F7KP0X", "D2L9MJ6A", "Y5C0BN3V", "T7Z1RD8E", "L4X6UJ9K",
            "H3V5ZN0T", "P9A2XE7L", "N6B8KT1M", "K0M3YW4C", "U7E5QL9Z",
            "R8T1BV2N", "S9X6FA0L", "Q2J3NK7Y", "J4C5MD8P", "E1V9HT6B"
        };
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

            string user = await _databaseService.GetUserViePromokod(code);


            return Json(new { promoCode = code, prize = $"{user} 🎁 IPhone 16 Pro Max 😍!" });

        }




        [HttpGet]
        public IActionResult Setting()
        {
            return View();  
        }
    }
}
