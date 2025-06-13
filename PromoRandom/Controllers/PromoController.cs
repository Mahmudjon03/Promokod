using Microsoft.AspNetCore.Mvc;
using PromoRandom.Services;

namespace PromoRandom.Controllers
{
    public class PromoController(DatabaseService databaseService) : Controller
    {

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetPromoCode()
        {
            var random = new Random();
            var activeGive = await databaseService.GetFirstIncompleteGiveAsync();

            if (activeGive == null)
            {
                return Json(new { error = "Нет активных розыгрышей" });
            }

            var promoCodes = (await databaseService.GetPromoCodesByGiveIdAsync(activeGive.Id))
                .Where(p => p.Status == "active")
                .ToList();

            if (promoCodes.Count == 0)
            {
                return Json(new { error = "Нет активных промо-кодов" });
            }

            var randomPromocode = promoCodes[random.Next(promoCodes.Count)];
            var user = await databaseService.GetUserByChatIdAsync(randomPromocode.UserChatId);

            //// Форматируем промо-код в 8 символов
            //string formattedCode = FormatPromoCode(randomPromocode.Code, 8);

            return Json(new
            {
                promoCode = randomPromocode.Code,
                userName = user?.Name ?? "Неизвестный"
            });
        }

        //private string FormatPromoCode(string code, int length)
        //{
        //    if (code.Length > length)
        //        return code.Substring(0, length);

        //    return code.PadRight(length, ' '); // Дополняем пробелами
        //}
    }
}
