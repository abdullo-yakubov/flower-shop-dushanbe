using FlowerShop.Infrastructure.Persistence.Repositories;

namespace FlowerShop.Infrastructure.Persistence;

public class UnitOfWork(AppDbContext context)
{
    public UserRepository Users { get; } = new(context);
    public ProductRepository Products { get; } = new(context);
    public OrderRepository Orders { get; } = new(context);

    public async Task<int> SaveChangesAsync(CancellationToken ct = default)
        => await context.SaveChangesAsync(ct);
}
