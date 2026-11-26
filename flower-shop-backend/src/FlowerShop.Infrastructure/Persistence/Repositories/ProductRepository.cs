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
            .AsNoTracking()
            .AsSplitQuery();

        if (!string.IsNullOrWhiteSpace(search))
        {
            var pattern = $"%{search.Trim().Replace("\\", "\\\\").Replace("%", "\\%").Replace("_", "\\_")}%";
            query = query.Where(p =>
                EF.Functions.ILike(p.NameRu, pattern) || EF.Functions.ILike(p.NameEn, pattern) ||
                EF.Functions.ILike(p.NameTg, pattern) || EF.Functions.ILike(p.CompositionRu, pattern));
        }
        if (!string.IsNullOrWhiteSpace(occasion) && Enum.TryParse<Occasion>(occasion, true, out var occ))
            query = query.Where(p => p.Occasions.Any(o => o.Occasion == occ));

        var parsedTypes = (flowerTypes ?? [])
            .Select(t => Enum.TryParse<FlowerType>(t, true, out var ft) ? ft : (FlowerType?)null)
            .Where(t => t.HasValue).Select(t => t!.Value).Distinct().ToList();
        if (parsedTypes.Count > 0)
            query = query.Where(p => p.FlowerTypes.Any(ft => parsedTypes.Contains(ft.FlowerType)));

        if (!string.IsNullOrWhiteSpace(packaging) &&
            Enum.TryParse<Packaging>(packaging.Replace("_", ""), true, out var pkg))
            query = query.Where(p => p.PackagingOptions.Any(o => o.Packaging == pkg));
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

        // ThenBy(Id) keeps paging stable when sort keys tie.
        query = sort switch
        {
            "price_asc" => query.OrderBy(p => p.Price).ThenBy(p => p.Id),
            "price_desc" => query.OrderByDescending(p => p.Price).ThenBy(p => p.Id),
            "newest" => query.OrderByDescending(p => p.CreatedAt).ThenBy(p => p.Id),
            "rating" => query.OrderByDescending(p => p.Rating).ThenBy(p => p.Id),
            _ => query.OrderByDescending(p => p.ReviewCount).ThenBy(p => p.SortOrder).ThenBy(p => p.Id)
        };

        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);
        var total = await query.CountAsync(ct);
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        return (items, total);
    }
}
