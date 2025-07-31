using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PromoRandom.Models;
using PromoRandom.Services;

namespace PromoRandom.Controllers;

[Authorize]
public class BroadcastController(
    DatabaseService databaseService,
    WinnerNotificationService notificationService,
    ImageService imageService) : Controller
{
    [HttpGet]
    public IActionResult SendBroadcast()
    {
        return View();
    }

    [HttpGet]
    public async Task<IActionResult> BroadcastHistory()
    {
        var logs = await databaseService.GetBroadcastLogsAsync();
        return View(logs);
    }

    [HttpPost]
    public async Task<IActionResult> SendBroadcast(string language, string message, IFormFile image)
    {
        string? photoUrl = null;

        if (image != null && image.Length > 0)
        {
            try
            {
                var imagePath = await imageService.SaveBroadcastImageAsync(image);
                photoUrl = $"{Request.Scheme}://{Request.Host}{imagePath}";
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("image", ex.Message);
                ViewBag.ErrorMessage = ex.Message;
                return View();
            }
        }

        var chatIds = language == "all"
            ? await databaseService.GetAllUserChatIdsAsync()
            : await databaseService.GetUserChatIdsByLanguageAsync(language);

        int successCount = 0;
        foreach (var chatId in chatIds)
        {
            if (await notificationService.SendBroadcastMessageAsync(chatId, message, photoUrl))
                successCount++;
            await Task.Delay(100);
        }

        await databaseService.SaveBroadcastLogAsync(new BroadcastLog
        {
            Message = message,
            PhotoUrl = photoUrl,
            SuccessCount = successCount,
            TotalCount = chatIds.Count,
            Language = language
        });

        ViewBag.SuccessCount = successCount;
        ViewBag.TotalCount = chatIds.Count;
        ViewBag.Message = message;
        ViewBag.PhotoUrl = photoUrl;
        ViewBag.Language = language;

        return View("BroadcastResult");
    }
}