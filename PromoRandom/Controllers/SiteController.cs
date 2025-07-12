using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using PromoRandom.Services;

namespace PromoRandom.Controllers
{
    public class SiteController(DatabaseService databaseService) : Controller
    {
        public IActionResult SetLanguage(string culture, string returnUrl = "/")
        {
            Response.Cookies.Append(
                CookieRequestCultureProvider.DefaultCookieName,
                CookieRequestCultureProvider.MakeCookieValue(new RequestCulture(culture)),
                new CookieOptions { Expires = DateTimeOffset.UtcNow.AddYears(1) }
            );

            return LocalRedirect(returnUrl);
        }

        public async Task<IActionResult> Index()
        {
            var activeGive = await databaseService.GetFirstActiveGiveAsync();
            if (activeGive == null)
            {
                return Json(new { Error = "Oops! Looks like no giveaways are active right now" });
            }

            ViewBag.EndDate = activeGive.EndDate;

            var prizes = await databaseService.GetPrizesByGiveIdAsync(activeGive.Id);
            ViewBag.Prizes = prizes;

            // Получаем последние 3 новости
            var latestNews = (await databaseService.GetAllArticlesAsync())
                .OrderByDescending(a => a.CreatedAt)
                .Take(3)
                .ToList();

            ViewBag.LatestNews = latestNews;

            return View();
        }

        public async Task<IActionResult> Statistics()
        {
            var stats = await databaseService.GetWinnerStatisticsAsync();
            return View(stats);
        }


        public async Task<IActionResult> News()
        {
            var articles = await databaseService.GetAllArticlesAsync();
            return View(articles);
        }

        public async Task<IActionResult> NewsDetails(int id)
        {
            var article = await databaseService.GetArticleByIdAsync(id);
            if (article == null)
            {
                return NotFound();
            }

            // Увеличиваем счетчик просмотров
            article.Views++;
            await databaseService.UpdateArticleAsync(article);

            return View(article);
        }
    }
}
