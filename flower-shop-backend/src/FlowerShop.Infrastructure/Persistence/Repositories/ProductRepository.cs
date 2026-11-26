using FlowerShop.Domain.Entities;
using FlowerShop.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace FlowerShop.Infrastructure.Persistence.Repositories;

public class ProductRepository(AppDbContext context) : BaseRepository<Product>(context)
{
    public async Task<Product?> GetBySlugAsync(string slug, CancellationToken ct = default)
        => await _dbSet
            .Include(p => p.Images)
            .Include(p => p.Sizes)
            .Include(p => p.Occasions)
            .Include(p => p.FlowerTypes)
            .Include(p => p.Colors)
            .Include(p => p.PackagingOptions)
            .FirstOrDefaultAsync(p => p.Slug == slug && p.IsActive, ct);

    public async Task<Product?> GetFullAsync(Guid id, CancellationToken ct = default)
        => await _dbSet
            .Include(p => p.Images)
            .Include(p => p.Sizes)
            .Include(p => p.Occasions)
            .Include(p => p.FlowerTypes)
            .Include(p => p.Colors)
            .Include(p => p.PackagingOptions)
            .FirstOrDefaultAsync(p => p.Id == id, ct);

    public async Task<(IEnumerable<Product> Items, int Total)> GetPagedAsync(
        string? search, string? occasion, IEnumerable<string>? flowerTypes, IEnumerable<string>? colors,
        int? priceMin, int? priceMax, bool? deliverToday, string? freshness, string? kind, string? packaging,
        string? sort, int page, int pageSize, CancellationToken ct = default)
    {
        var query = _dbSet
            .Include(p => p.Images)
            .Include(p => p.Sizes)
            .Include(p => p.Occasions)
            .Include(p => p.FlowerTypes)
            .Include(p => p.Colors)
            .Include(p => p.PackagingOptions)
            .Where(p => p.IsActive)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(p => p.NameRu.Contains(search) || p.CompositionRu.Contains(search));
        if (!string.IsNullOrWhiteSpace(occasion) && Enum.TryParse<Occasion>(occasion, true, out var occ))
            query = query.Where(p => p.Occasions.Any(o => o.Occasion == occ));
        if (flowerTypes?.Any() == true)
            query = query.Where(p => p.FlowerTypes.Any(ft => flowerTypes.Contains(ft.FlowerType.ToString().ToLower())));
        if (colors?.Any() == true)
            query = query.Where(p => p.Colors.Any(c => colors.Contains(c.Color)));
        if (priceMin.HasValue)
            query = query.Where(p => p.Price >= priceMin.Value);
        if (priceMax.HasValue)
            query = query.Where(p => p.Price <= priceMax.Value);
        if (deliverToday == true)
            query = query.Where(p => p.DeliverToday);
        if (!string.IsNullOrWhiteSpace(freshness) && Enum.TryParse<Freshness>(freshness, true, out var fr))
            query = query.Where(p => p.Freshness == fr);
        if (!string.IsNullOrWhiteSpace(kind) && Enum.TryParse<ProductKind>(kind, true, out var pk))
            query = query.Where(p => p.Kind == pk);

        query = sort switch
        {
            "price_asc" => query.OrderBy(p => p.Price),
            "price_desc" => query.OrderByDescending(p => p.Price),
            "newest" => query.OrderByDescending(p => p.CreatedAt),
            "rating" => query.OrderByDescending(p => p.Rating),
            _ => query.OrderByDescending(p => p.ReviewCount).ThenBy(p => p.SortOrder)
        };

        var total = await query.CountAsync(ct);
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        return (items, total);
    }
}
