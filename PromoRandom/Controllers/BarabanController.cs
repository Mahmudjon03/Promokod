using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PromoRandom.Models;
using PromoRandom.Services;

namespace PromoRandom.Controllers
{
    [Authorize]
    public class BarabanController(DatabaseService databaseService, WinnerNotificationService winnerNotification) : Controller
    {
        [HttpGet]
        public IActionResult NoActiveGive()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GiveEnded()
        {
            var activeGive = await databaseService.GetFirstActiveGiveAsync();
            var givePromocodes = (await databaseService.GetAllPromoCodesAsync()).Where(p => p.GiveId == activeGive.Id && p.Status == "active");
            foreach (var item in givePromocodes)
            {
                await databaseService.UpdatePromoCodeStatusAsync(item.Id, "expired");
            }
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var activeGive = await databaseService.GetFirstActiveGiveAsync();
            if (activeGive == null)
            {
                return View("NoActiveGive");
            }

            // Получаем призы с информацией о победителях
            List<ViewModels.PrizeViewModel> prizes = await databaseService.GetPrizesWithWinnersAsync(activeGive.Id);

            // Определяем следующий приз для розыгрыша (только с доступными экземплярами)
            var nextPrize = prizes
                .Where(p => p.WonCount < p.Quantity) // Только призы с доступными экземплярами
                .OrderBy(p => p.Priority)
                .ThenBy(p => p.SpinDuration)
                .FirstOrDefault();

            if (nextPrize != null)
            {
                nextPrize.Status = "in-progress";
            }
            else
            {
                return View("GiveEnded");
            }

            ViewBag.CurrentPrize = nextPrize;
            ViewBag.Prizes = prizes;

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> GetPromoCode()
        {
            try
            {
                var random = new Random();
                var activeGive = await databaseService.GetFirstActiveGiveAsync();

                if (activeGive == null)
                    return Json(new { error = "Нет активных розыгрышей" });

                var prizes = await databaseService.GetAvailablePrizesByGiveIdAsync(activeGive.Id);

                var prizeToWin = prizes
                    .Where(p => p.IsAvailable)
                    .OrderBy(p => p.Priority)
                    .ThenBy(p => p.SpinDuration)
                    .FirstOrDefault();

                if (prizeToWin == null)
                    return Json(new { error = "Нет призов для розыгрыша" });

                // Получаем только промокоды от пользователей с достаточным количеством
                var promoCodes = await databaseService.GetPromoCodesByGiveIdAsync(
                    activeGive.Id,
                    prizeToWin.RequiredPromoCodeCount);

                if (promoCodes.Count == 0)
                    return Json(new { error = $"Нет подходящих промо-кодов (требуется минимум {prizeToWin.RequiredPromoCodeCount} от одного пользователя)" });

                var randomPromoCode = promoCodes[random.Next(promoCodes.Count)];
                var user = await databaseService.GetUserByChatIdAsync(randomPromoCode.UserChatId);

                if (user == null)
                    return Json(new { error = "Пользователь не найден" });

                await NotifyAdminsAsync(prizeToWin.Name, user);

                return Json(new
                {
                    promoCode = randomPromoCode.Code,
                    userChatId = user.ChatId,
                    userName = user.Name,
                    prizeName = prizeToWin.Name,
                    prizeImage = prizeToWin.ImagePath,
                    prizeId = prizeToWin.Id,
                    promoCodeId = randomPromoCode.Id,
                    spinDuration = prizeToWin.SpinDuration,
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.Message);
                return Json(new { error = "Произошла ошибка при обработке запроса" });
            }
        }

        [HttpPost]
        public async Task<IActionResult> ConfirmPrize([FromBody] ConfirmPrizeRequest model)
        {

            try
            {
                // 1. Добавляем победителя
                await databaseService.AddWinnerAsync(model.PromoCodeId, model.PrizeId);

                // 2. Обновляем статус промокода
                await databaseService.UpdatePromoCodeStatusAsync(model.PromoCodeId, "won");

                // 3. Увеличиваем счетчик призов
                await databaseService.IncrementPrizeWinCountAsync(model.PrizeId);

                // 4.
                var prize = await databaseService.GetPrizeByIdAsync(model.PrizeId);
                var user = await databaseService.GetUserByChatIdAsync(model.UserChatId);

                await NotifyUserAsync(prize.Name, user);

                if (prize.RequiredPromoCodeCount > 1)
                {
                    var userPromoCodes = await databaseService.GetAllPromoCodesForUserAsync(model.UserChatId, "active");
                    Random random = new();
                    var randomPromocode = userPromoCodes[random.Next(userPromoCodes.Count)];
                    await databaseService.UpdatePromoCodeStatusAsync(randomPromocode.Id, "won");
                }
                return Ok();

            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Ошибка: {ex.Message}");
            }
        }

        public class ConfirmPrizeRequest
        {
            public int PromoCodeId { get; set; }
            public int PrizeId { get; set; }
            public long UserChatId { get; set; }
        }

        private async Task NotifyAdminsAsync(string prizeName, User winner)
        {
            var adminChatIds = await databaseService.GetAllAdminChatIdsAsync();

            foreach (var adminChatId in adminChatIds)
            {
                var message = $"🏆 Приз: {prizeName}\n" +
                              $"👤 Победитель: {winner.Name ?? "Имя не указано"}\n" +
                              $"📱 Телефон: {winner.Phone ?? "не указан"}\n" +
                              $"💬 Язык: {winner.Language}";

                await winnerNotification.SendAdminNotificationAsync(adminChatId, message);
            }
        }


        private async Task NotifyUserAsync(string prizeName, User user)
        {
            await winnerNotification.SendWinnerMessageAsync(user.ChatId, user.Language, prizeName);
        }
    }
}