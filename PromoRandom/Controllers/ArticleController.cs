using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PromoRandom.Models;
using PromoRandom.Services;

namespace PromoRandom.Controllers;

[Authorize]
public class ArticleController(DatabaseService databaseService, ImageService imageService) : Controller
{
    [HttpGet]
    public async Task<IActionResult> Index()
    {
        var articles = await databaseService.GetAllArticlesAsync();
        return View(articles);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View();
    }

    [HttpPost]
    public async Task<IActionResult> Create(Article article, IFormFile imageFile)
    {
        if (!ModelState.IsValid) return View(article);

        try
        {
            if (imageFile != null && imageFile.Length > 0)
            {
                article.ImagePath = await imageService.SaveArticleImageAsync(imageFile);
            }

            await databaseService.CreateArticleAsync(article);
            return RedirectToAction("Index");
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError("ImageFile", ex.Message);
            return View(article);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var article = await databaseService.GetArticleByIdAsync(id);
        return article == null ? NotFound() : View(article);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(Article article, IFormFile imageFile)
    {
        if (!ModelState.IsValid) return View(article);

        try
        {
            if (imageFile != null && imageFile.Length > 0)
            {
                // Удаляем старое изображение
                if (!string.IsNullOrEmpty(article.ImagePath))
                {
                    imageService.DeleteImage(article.ImagePath);
                }

                article.ImagePath = await imageService.SaveArticleImageAsync(imageFile);
            }

            await databaseService.UpdateArticleAsync(article);
            return RedirectToAction("Index");
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError("ImageFile", ex.Message);
            return View(article);
        }
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        var article = await databaseService.GetArticleByIdAsync(id);
        if (article == null) return NotFound();

        if (!string.IsNullOrEmpty(article.ImagePath))
        {
            imageService.DeleteImage(article.ImagePath);
        }

        await databaseService.DeleteArticleAsync(id);
        return RedirectToAction("Index");
    }
}