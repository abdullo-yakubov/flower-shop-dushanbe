using FlowerShop.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace FlowerShop.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<OtpCode> OtpCodes => Set<OtpCode>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductImage> ProductImages => Set<ProductImage>();
    public DbSet<ProductSize> ProductSizes => Set<ProductSize>();
    public DbSet<ProductOccasion> ProductOccasions => Set<ProductOccasion>();
    public DbSet<ProductFlowerType> ProductFlowerTypes => Set<ProductFlowerType>();
    public DbSet<ProductColor> ProductColors => Set<ProductColor>();
    public DbSet<ProductPackaging> ProductPackagings => Set<ProductPackaging>();
    public DbSet<Addon> Addons => Set<Addon>();
    public DbSet<Postcard> Postcards => Set<Postcard>();
    public DbSet<MessageTemplate> MessageTemplates => Set<MessageTemplate>();
    public DbSet<PromoCode> PromoCodes => Set<PromoCode>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();
    public DbSet<OrderAddon> OrderAddons => Set<OrderAddon>();
    public DbSet<OrderStatusHistory> OrderStatusHistories => Set<OrderStatusHistory>();
    public DbSet<LoyaltyAccount> LoyaltyAccounts => Set<LoyaltyAccount>();
    public DbSet<LoyaltyTransaction> LoyaltyTransactions => Set<LoyaltyTransaction>();
    public DbSet<Review> Reviews => Set<Review>();
    public DbSet<ReviewPhoto> ReviewPhotos => Set<ReviewPhoto>();
    public DbSet<Address> Addresses => Set<Address>();
    public DbSet<DeliveryZone> DeliveryZones => Set<DeliveryZone>();
    public DbSet<HolidayPricing> HolidayPricings => Set<HolidayPricing>();
    public DbSet<Setting> Settings => Set<Setting>();
    public DbSet<PushSubscription> PushSubscriptions => Set<PushSubscription>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}
