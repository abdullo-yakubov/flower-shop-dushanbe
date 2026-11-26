using FlowerShop.Infrastructure.Services;
using Microsoft.AspNetCore.Http;
using SkiaSharp;

namespace FlowerShop.Tests;

public sealed class FileStorageTests : IDisposable
{
    private readonly string _dir = Path.Combine(Path.GetTempPath(), "flowershop-tests-" + Guid.NewGuid().ToString("N"));

    private static IFormFile PngFile(int width, int height)
    {
        using var bitmap = new SKBitmap(width, height);
        bitmap.Erase(SKColors.HotPink);
        using var data = SKImage.FromBitmap(bitmap).Encode(SKEncodedImageFormat.Png, 100);
        var stream = new MemoryStream(data.ToArray());
        return new FormFile(stream, 0, stream.Length, "file", "photo.png");
    }

    [Fact]
    public async Task Saves_downscaled_webp_and_deletes_it()
    {
        var storage = new LocalFileStorageService(_dir);
        var url = await storage.SaveAsync(PngFile(1200, 800), "avatars", 600, 800);

        Assert.StartsWith("/uploads/avatars/", url);
        Assert.EndsWith(".webp", url);

        var path = Path.Combine(_dir, "avatars", Path.GetFileName(url));
        using (var saved = SKBitmap.Decode(path))
        {
            Assert.Equal(600, saved.Width);
            Assert.Equal(400, saved.Height);
        }

        await storage.DeleteAsync(url);
        Assert.False(File.Exists(path));
    }

    [Fact]
    public async Task Rejects_non_images()
    {
        var stream = new MemoryStream("not an image"u8.ToArray());
        var file = new FormFile(stream, 0, stream.Length, "file", "x.png");
        await Assert.ThrowsAsync<AppException>(() => new LocalFileStorageService(_dir).SaveAsync(file, "avatars"));
    }

    [Fact]
    public async Task Delete_cannot_escape_uploads_directory()
    {
        var outside = Path.Combine(Path.GetTempPath(), $"keep-{Guid.NewGuid():N}.txt");
        await File.WriteAllTextAsync(outside, "keep");
        try
        {
            var storage = new LocalFileStorageService(_dir);
            await Assert.ThrowsAsync<AppException>(() => storage.DeleteAsync($"/uploads/../{Path.GetFileName(outside)}"));
            Assert.True(File.Exists(outside));
        }
        finally
        {
            File.Delete(outside);
        }
    }

    public void Dispose()
    {
        if (Directory.Exists(_dir)) Directory.Delete(_dir, recursive: true);
    }
}
