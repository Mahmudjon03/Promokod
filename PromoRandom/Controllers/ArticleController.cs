using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PromoRandom.Models;
using PromoRandom.Services;

namespace PromoRandom.Controllers;

[Authorize]
public class ArticleController(DatabaseService databaseService, IWebHostEnvironment env) : Controller
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
        if (!ModelState.IsValid)
        {
            return View(article);
        }

        // Обработка изображения
        if (imageFile != null && imageFile.Length > 0)
        {
            var imagePath = await SaveImageAsync(imageFile);
            article.ImagePath = imagePath;
        }

        await databaseService.CreateArticleAsync(article);
        return RedirectToAction("Index");
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var article = await databaseService.GetArticleByIdAsync(id);
        if (article == null)
        {
            return NotFound();
        }
        return View(article);
    }

    [HttpPost]
    public async Task<IActionResult> Edit(Article article, IFormFile imageFile)
    {
        if (!ModelState.IsValid)
        {
            return View(article);
        }

        // Обновление изображения
        if (imageFile != null && imageFile.Length > 0)
        {
            // Удаляем старое изображение
            if (!string.IsNullOrEmpty(article.ImagePath))
            {
                DeleteImage(article.ImagePath);
            }

            var imagePath = await SaveImageAsync(imageFile);
            article.ImagePath = imagePath;
        }

        await databaseService.UpdateArticleAsync(article);
        return RedirectToAction("Index");
    }

    [HttpPost]
    public async Task<IActionResult> Delete(int id)
    {
        var article = await databaseService.GetArticleByIdAsync(id);
        if (article != null && !string.IsNullOrEmpty(article.ImagePath))
        {
            DeleteImage(article.ImagePath);
        }

        await databaseService.DeleteArticleAsync(id);
        return RedirectToAction("Index");
    }

    private async Task<string> SaveImageAsync(IFormFile imageFile)
    {
        var uploadsFolder = Path.Combine(env.WebRootPath, "uploads", "articles");
        if (!Directory.Exists(uploadsFolder))
        {
            Directory.CreateDirectory(uploadsFolder);
        }

        var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
        var filePath = Path.Combine(uploadsFolder, uniqueFileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await imageFile.CopyToAsync(stream);
        }

        return $"/uploads/articles/{uniqueFileName}";
    }

    private void DeleteImage(string imagePath)
    {
        var filePath = Path.Combine(env.WebRootPath, imagePath.TrimStart('/'));
        if (System.IO.File.Exists(filePath))
        {
            System.IO.File.Delete(filePath);
        }
    }
}