using FlowerShop.Domain.Entities;
using FlowerShop.Domain.Enums;
using Microsoft.EntityFrameworkCore;

namespace FlowerShop.Infrastructure.Persistence.Repositories;

public class UserRepository(AppDbContext context) : BaseRepository<User>(context)
{
    public async Task<User?> GetByPhoneAsync(string phone, CancellationToken ct = default)
        => await _dbSet.FirstOrDefaultAsync(u => u.Phone == phone, ct);

    public async Task<User?> GetWithLoyaltyAsync(Guid id, CancellationToken ct = default)
        => await _dbSet.Include(u => u.LoyaltyAccount).FirstOrDefaultAsync(u => u.Id == id, ct);

    public async Task<(IEnumerable<User> Items, int Total)> GetPagedAsync(
        string? search, UserRole? role, bool? isBlocked, int page, int pageSize, CancellationToken ct = default)
    {
        var query = _dbSet.AsQueryable();

        if (!string.IsNullOrWhiteSpace(search))
            query = query.Where(u => u.Phone.Contains(search) || u.Name.Contains(search));
        if (role.HasValue)
            query = query.Where(u => u.Role == role.Value);
        if (isBlocked.HasValue)
            query = query.Where(u => u.IsBlocked == isBlocked.Value);

        var total = await query.CountAsync(ct);
        var items = await query.OrderByDescending(u => u.CreatedAt)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);

        return (items, total);
    }
}
