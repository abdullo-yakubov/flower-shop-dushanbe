using Microsoft.AspNetCore.Http;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace FlowerShop.Infrastructure.Services;

public interface IFileStorageService
{
    Task<string> SaveAsync(IFormFile file, string folder, int maxWidth = 600, int maxHeight = 800, CancellationToken ct = default);
    Task DeleteAsync(string url, CancellationToken ct = default);
}

public class LocalFileStorageService : IFileStorageService
{
    private readonly string _baseDir;
    private readonly string _baseUrl;

    public LocalFileStorageService(string baseDir, string baseUrl = "/uploads")
    {
        _baseDir = baseDir;
        _baseUrl = baseUrl;
        Directory.CreateDirectory(_baseDir);
    }

    public async Task<string> SaveAsync(IFormFile file, string folder, int maxWidth = 600, int maxHeight = 800, CancellationToken ct = default)
    {
        var dir = Path.Combine(_baseDir, folder);
        Directory.CreateDirectory(dir);

        var fileName = $"{Guid.NewGuid()}.webp";
        var filePath = Path.Combine(dir, fileName);

        using var image = await Image.LoadAsync(file.OpenReadStream(), ct);
        image.Mutate(x => x.Resize(new ResizeOptions
        {
            Size = new Size(maxWidth, maxHeight),
            Mode = ResizeMode.Max
        }));
        await image.SaveAsWebpAsync(filePath, ct);

        return $"{_baseUrl}/{folder}/{fileName}";
    }

    public Task DeleteAsync(string url, CancellationToken ct = default)
    {
        var relative = url.Replace(_baseUrl, string.Empty).TrimStart('/');
        var path = Path.Combine(_baseDir, relative.Replace('/', Path.DirectorySeparatorChar));
        if (File.Exists(path))
            File.Delete(path);
        return Task.CompletedTask;
    }
}
