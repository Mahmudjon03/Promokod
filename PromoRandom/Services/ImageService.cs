using PromoRandom.Models;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;

namespace PromoRandom.Services;

public class ImageService(IWebHostEnvironment environment)
{
    private const int DefaultMaxSizeKb = 500;
    private const int MaxImageSizeBytes = 5 * 1024 * 1024; // 5MB

    public async Task<string> SaveBroadcastImageAsync(IFormFile imageFile)
    {
        ValidateImage(imageFile, [".jpg", ".jpeg", ".png", ".gif", ".webp"]);
        return await SaveImageAsync(imageFile, "uploads/broadcasts", Guid.NewGuid().ToString());
    }

    public async Task<string> SaveArticleImageAsync(IFormFile imageFile)
    {
        ValidateImage(imageFile, [".jpg", ".jpeg", ".png", ".webp"]);
        return await SaveImageAsync(imageFile, "uploads/articles", Guid.NewGuid().ToString());
    }

    public async Task<string> SavePrizeImageAsync(IFormFile imageFile, Prize prize)
    {
        ValidateImage(imageFile, [".jpg", ".jpeg", ".png", ".gif", ".webp"]);
        var fileName = $"prize{prize.Id}_{prize.GiveId}";
        return await SaveImageAsync(imageFile, "uploads/prizes", fileName);
    }

    private async Task<string> SaveImageAsync(IFormFile imageFile, string folder, string fileNameBase)
    {
        var extension = Path.GetExtension(imageFile.FileName).ToLower();
        var fileName = $"{fileNameBase}{extension}";
        var relativePath = $"{folder}/{fileName}";
        var fullPath = Path.Combine(environment.WebRootPath, relativePath);

        Directory.CreateDirectory(Path.GetDirectoryName(fullPath)!);

        await using var processedStream = await ProcessImageAsync(imageFile);
        await using var fileStream = new FileStream(fullPath, FileMode.Create);
        await processedStream.CopyToAsync(fileStream);

        return $"/{relativePath}";
    }

    private static async Task<Stream> ProcessImageAsync(IFormFile imageFile)
    {
        if (imageFile.Length <= DefaultMaxSizeKb * 1024 &&
            (imageFile.ContentType == "image/png" || imageFile.ContentType == "image/gif"))
        {
            return imageFile.OpenReadStream();
        }

        using var image = await Image.LoadAsync(imageFile.OpenReadStream());

        if (image.Width > 1200 || image.Height > 1200)
        {
            image.Mutate(x => x.Resize(new ResizeOptions
            {
                Mode = ResizeMode.Max,
                Size = new Size(1200)
            }));
        }

        var outputStream = new MemoryStream();

        if (imageFile.ContentType == "image/png")
        {
            await image.SaveAsPngAsync(outputStream, new PngEncoder
            {
                CompressionLevel = PngCompressionLevel.BestCompression
            });
        }
        else
        {
            var quality = CalculateJpegQuality(imageFile.Length);
            await image.SaveAsJpegAsync(outputStream, new JpegEncoder { Quality = quality });
        }

        outputStream.Position = 0;
        return outputStream;
    }

    private void ValidateImage(IFormFile imageFile, string[] allowedExtensions)
    {
        var extension = Path.GetExtension(imageFile.FileName).ToLower();

        if (!allowedExtensions.Contains(extension))
            throw new InvalidOperationException($"Допустимы только {string.Join(", ", allowedExtensions)} изображения");

        if (imageFile.Length > MaxImageSizeBytes)
            throw new InvalidOperationException($"Максимальный размер изображения - {MaxImageSizeBytes / 1024 / 1024}MB");
    }

    private static int CalculateJpegQuality(long fileSize)
    {
        const int maxSizeBytes = DefaultMaxSizeKb * 1024;
        double ratio = (double)maxSizeBytes / fileSize;
        int quality = (int)(85 * ratio);
        return Math.Clamp(quality, 60, 95);
    }

    public void DeleteImage(string imagePath)
    {
        var fullPath = Path.Combine(environment.WebRootPath, imagePath.TrimStart('/'));
        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }
    }
}