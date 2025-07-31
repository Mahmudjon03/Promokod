using Microsoft.AspNetCore.Localization;
using Microsoft.AspNetCore.Mvc;
using PromoRandom.Models;
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
            // Получаем наиболее релевантный розыгрыш
            var relevantGive = await databaseService.GetRelevantGiveAsync();

            // Определяем статус розыгрыша
            string giveawayStatus = "none";
            DateTime? targetDate = null;

            if (relevantGive != null)
            {
                if (DateTime.Now >= relevantGive.StartDate && DateTime.Now <= relevantGive.EndDate)
                {
                    giveawayStatus = "active";
                    targetDate = relevantGive.EndDate;
                }
                else if (DateTime.Now < relevantGive.StartDate)
                {
                    giveawayStatus = "upcoming";
                    targetDate = relevantGive.StartDate;
                }
                // Для завершенных, но не отмеченных is_completed
                else if (DateTime.Now > relevantGive.EndDate)
                {
                    giveawayStatus = "finished";
                }
            }

            ViewBag.GiveawayStatus = giveawayStatus;
            ViewBag.TargetDate = targetDate;
            ViewBag.Give = relevantGive;

            // Получаем призы, если есть релевантный розыгрыш
            List<Prize> prizes = null;
            if (relevantGive != null)
            {
                prizes = await databaseService.GetPrizesByGiveIdAsync(relevantGive.Id);
            }
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

        [Route("site/promo/{chatId:long}")]
        public async Task<IActionResult> Promo(long chatId)
        {
            var promos = await databaseService.GetUserPromoDetailsAsync(chatId);
            return View(promos);
        }

    }
}
