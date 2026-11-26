using FlowerShop.Domain.Entities;
using FlowerShop.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace FlowerShop.Infrastructure.Persistence.Repositories;

public class OrderRepository(AppDbContext context) : BaseRepository<Order>(context)
{
    public async Task<Order?> GetFullAsync(Guid id, CancellationToken ct = default)
        => await _dbSet
            .Include(o => o.Items).ThenInclude(i => i.Product)
            .Include(o => o.Addons).ThenInclude(a => a.Addon)
            .Include(o => o.StatusHistory).ThenInclude(h => h.Admin)
            .Include(o => o.User)
            .Include(o => o.DeliveryZone)
            .Include(o => o.Postcard)
            .FirstOrDefaultAsync(o => o.Id == id, ct);

    public async Task<(IEnumerable<Order> Items, int Total)> GetUserOrdersAsync(
        Guid userId, string? status, int page, int pageSize, CancellationToken ct = default)
    {
        var query = _dbSet.Where(o => o.UserId == userId);
        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<OrderStatus>(status, true, out var s))
            query = query.Where(o => o.Status == s);

        var total = await query.CountAsync(ct);
        var items = await query.Include(o => o.Items).OrderByDescending(o => o.CreatedAt)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        return (items, total);
    }

    public async Task<(IEnumerable<Order> Items, int Total)> GetAdminPagedAsync(
        string? search, string? status, DateOnly? dateFrom, DateOnly? dateTo, string? paymentMethod,
        int page, int pageSize, CancellationToken ct = default)
    {
        var query = _dbSet.Include(o => o.Items).AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(o => o.OrderNumber.Contains(search) || o.CustomerPhone.Contains(search) || o.CustomerName.Contains(search));
        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<OrderStatus>(status, true, out var s))
            query = query.Where(o => o.Status == s);
        if (dateFrom.HasValue)
            query = query.Where(o => o.DeliveryDate >= dateFrom.Value);
        if (dateTo.HasValue)
            query = query.Where(o => o.DeliveryDate <= dateTo.Value);
        if (!string.IsNullOrWhiteSpace(paymentMethod) && Enum.TryParse<PaymentMethod>(paymentMethod, true, out var pm))
            query = query.Where(o => o.PaymentMethod == pm);

        var total = await query.CountAsync(ct);
        var items = await query.OrderByDescending(o => o.CreatedAt)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);
        return (items, total);
    }

    public async Task<string> GenerateOrderNumberAsync(CancellationToken ct = default)
    {
        var count = await _dbSet.CountAsync(ct);
        return $"#{(count + 1):D6}";
    }
}
