using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PromoRandom.Services;

namespace PromoRandom.Controllers
{
    [Authorize]
    public class BroadcastController(
        DatabaseService databaseService,
        WinnerNotificationService notificationService,
        IWebHostEnvironment env) : Controller
    {
        [HttpGet]
        public IActionResult SendBroadcast()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SendBroadcast(string message, IFormFile image)
        {
            string? photoUrl = null;

            // Обработка загруженного изображения
            if (image != null && image.Length > 0)
            {
                // Проверяем тип файла
                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png" };
                var extension = Path.GetExtension(image.FileName).ToLowerInvariant();

                if (!allowedExtensions.Contains(extension))
                {
                    ModelState.AddModelError("image", "Допустимы только JPG, JPEG и PNG изображения");
                    return View();
                }

                var uploadsFolder = Path.Combine(env.WebRootPath, "uploads");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var uniqueFileName = Guid.NewGuid().ToString() + "_" + image.FileName;
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await image.CopyToAsync(stream);
                }

                photoUrl = $"{Request.Scheme}://{Request.Host}/uploads/{uniqueFileName}";
            }

            // Получаем всех пользователей
            var chatIds = await databaseService.GetAllUserChatIdsAsync();
            int successCount = 0;
            int totalCount = chatIds.Count;

            // Рассылка сообщений
            foreach (var chatId in chatIds)
            {
                var success = await notificationService.SendBroadcastMessageAsync(chatId, message, photoUrl);
                if (success) successCount++;

                // Задержка, чтобы не превысить лимиты Telegram
                await Task.Delay(100);
            }

            ViewBag.SuccessCount = successCount;
            ViewBag.TotalCount = totalCount;
            ViewBag.Message = message;
            ViewBag.PhotoUrl = photoUrl;

            return View("BroadcastResult");
        }
    }
}