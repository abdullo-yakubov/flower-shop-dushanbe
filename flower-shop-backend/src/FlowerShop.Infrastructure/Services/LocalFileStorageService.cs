using Microsoft.AspNetCore.Http;
using SkiaSharp;

namespace FlowerShop.Infrastructure.Services;

public interface IFileStorageService
{
    Task<string> SaveAsync(IFormFile file, string folder, int maxWidth = 600, int maxHeight = 800, CancellationToken ct = default);
    Task DeleteAsync(string url, CancellationToken ct = default);
}

/// <summary>
/// Stores images on local disk as WebP, downscaled to fit maxWidth×maxHeight.
/// Uses SkiaSharp (MIT) — ImageSharp 3.2+ requires a commercial license for Release builds.
/// </summary>
public class LocalFileStorageService : IFileStorageService
{
    public const long MaxUploadBytes = 10 * 1024 * 1024;
    private const int WebpQuality = 82;

    private readonly string _baseDir;
    private readonly string _baseUrl;

    public LocalFileStorageService(string baseDir, string baseUrl = "/uploads")
    {
        _baseDir = Path.GetFullPath(baseDir);
        _baseUrl = baseUrl.TrimEnd('/');
        Directory.CreateDirectory(_baseDir);
    }

    public async Task<string> SaveAsync(IFormFile file, string folder, int maxWidth = 600, int maxHeight = 800, CancellationToken ct = default)
    {
        if (file.Length is 0 or > MaxUploadBytes)
            throw new AppException("VALIDATION_ERROR", "Файл пустой или больше 10 МБ");

        var dir = ResolveInsideBase(folder);
        Directory.CreateDirectory(dir);

        await using var buffer = new MemoryStream();
        await file.CopyToAsync(buffer, ct);
        buffer.Position = 0;

        using var source = SKBitmap.Decode(buffer)
            ?? throw new AppException("VALIDATION_ERROR", "Файл не является изображением");

        var scale = Math.Min(1.0, Math.Min((double)maxWidth / source.Width, (double)maxHeight / source.Height));
        var width = Math.Max(1, (int)Math.Round(source.Width * scale));
        var height = Math.Max(1, (int)Math.Round(source.Height * scale));

        using var resized = scale < 1.0
            ? source.Resize(new SKImageInfo(width, height), new SKSamplingOptions(SKCubicResampler.Mitchell))
            : null;
        using var image = SKImage.FromBitmap(resized ?? source);
        using var encoded = image.Encode(SKEncodedImageFormat.Webp, WebpQuality)
            ?? throw new AppException("VALIDATION_ERROR", "Не удалось обработать изображение");

        var fileName = $"{Guid.NewGuid():N}.webp";
        await using (var output = File.Create(Path.Combine(dir, fileName)))
            encoded.SaveTo(output);

        return $"{_baseUrl}/{folder.Trim('/')}/{fileName}";
    }

    public Task DeleteAsync(string url, CancellationToken ct = default)
    {
        if (!url.StartsWith(_baseUrl + "/", StringComparison.Ordinal)) return Task.CompletedTask;

        var path = ResolveInsideBase(url[(_baseUrl.Length + 1)..]);
        if (File.Exists(path))
            File.Delete(path);
        return Task.CompletedTask;
    }

    /// <summary>Prevents "../" from escaping the uploads directory.</summary>
    private string ResolveInsideBase(string relative)
    {
        var full = Path.GetFullPath(Path.Combine(_baseDir, relative.Replace('/', Path.DirectorySeparatorChar)));
        if (!full.StartsWith(_baseDir + Path.DirectorySeparatorChar, StringComparison.Ordinal) && full != _baseDir)
            throw new AppException("VALIDATION_ERROR", "Недопустимый путь к файлу");
        return full;
    }
}
