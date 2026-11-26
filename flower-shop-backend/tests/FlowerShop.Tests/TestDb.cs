using FlowerShop.Domain.Entities;
using FlowerShop.Domain.Enums;
using FlowerShop.Infrastructure.Persistence;
using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;

namespace FlowerShop.Tests;

/// <summary>The InMemory provider can't store JsonDocument (jsonb in Postgres) — map it to a string.</summary>
internal sealed class InMemoryAppDbContext(DbContextOptions<AppDbContext> options) : AppDbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        var converter = new ValueConverter<JsonDocument, string>(
            d => d.RootElement.GetRawText(), s => JsonDocument.Parse(s, default));
        modelBuilder.Entity<OrderItem>().Property(i => i.ProductSnapshot).HasConversion(converter);
        modelBuilder.Entity<OrderAddon>().Property(a => a.AddonSnapshot).HasConversion(converter);
    }
}

/// <summary>Isolated in-memory database with a small, predictable catalog.</summary>
internal sealed class TestDb : IDisposable
{
    public AppDbContext Db { get; }
    public Guid UserId { get; } = Guid.NewGuid();

    public Product Bouquet { get; }
    public ProductSize BouquetM { get; }
    public Product Tulips { get; }
    public ProductSize TulipsSize { get; }
    public Product Dried { get; }
    public ProductSize DriedSize { get; }
    public Addon Chocolate { get; }
    public PromoCode Percent10 { get; }
    public PromoCode Fixed500 { get; }

    public TestDb()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        Db = new InMemoryAppDbContext(options);

        Bouquet = NewProduct("roses", ProductKind.Bouquet, Freshness.Live, Packaging.Basket, Packaging.NoBasket);
        BouquetM = NewSize(Bouquet, SizeLabel.M, 600);

        Tulips = NewProduct("tulips", ProductKind.Single, Freshness.Live, Packaging.NoBasket);
        Tulips.StemPrice = 12;
        Tulips.StemOptions = [11, 25, 51];
        TulipsSize = NewSize(Tulips, SizeLabel.M, 132);

        Dried = NewProduct("dried", ProductKind.Bouquet, Freshness.Dried, Packaging.NoBasket);
        DriedSize = NewSize(Dried, SizeLabel.M, 400);

        Chocolate = new Addon { Id = Guid.NewGuid(), Type = "chocolate", NameRu = "Шоколад", Price = 90, IsActive = true };
        Db.Addons.Add(Chocolate);

        Percent10 = NewPromo("FLOWER10", percent: 10);
        Fixed500 = NewPromo("MINUS500", fixedAmount: 500);

        Db.Users.Add(new User { Id = UserId, Phone = "+992900000001", Name = "Test" });
        Db.SaveChanges();
    }

    public void SetPoints(int points)
    {
        Db.LoyaltyAccounts.Add(new LoyaltyAccount { Id = Guid.NewGuid(), UserId = UserId, Points = points, TotalEarned = points });
        Db.SaveChanges();
    }

    public void AddHoliday(DateOnly date, int markup, string appliesTo = "live")
    {
        Db.HolidayPricings.Add(new HolidayPricing
        {
            Id = Guid.NewGuid(), NameRu = "Праздник", Month = date.Month, Day = date.Day,
            MarkupPercent = markup, AppliesTo = appliesTo, IsActive = true
        });
        Db.SaveChanges();
    }

    private Product NewProduct(string slug, ProductKind kind, Freshness freshness, params Packaging[] packaging)
    {
        var product = new Product
        {
            Id = Guid.NewGuid(), Slug = slug, NameRu = slug, Kind = kind, Freshness = freshness,
            InStock = true, IsActive = true
        };
        foreach (var p in packaging)
            product.PackagingOptions.Add(new ProductPackaging { ProductId = product.Id, Packaging = p });
        Db.Products.Add(product);
        return product;
    }

    private ProductSize NewSize(Product product, SizeLabel label, int price)
    {
        var size = new ProductSize { Id = Guid.NewGuid(), ProductId = product.Id, Label = label, Price = price };
        Db.ProductSizes.Add(size);
        return size;
    }

    private PromoCode NewPromo(string code, int percent = 0, int fixedAmount = 0)
    {
        var promo = new PromoCode
        {
            Id = Guid.NewGuid(), Code = code, LabelRu = code, DiscountPercent = percent, DiscountFixed = fixedAmount,
            ValidFrom = DateTimeOffset.UtcNow.AddDays(-1), IsActive = true
        };
        Db.PromoCodes.Add(promo);
        return promo;
    }

    public void Dispose() => Db.Dispose();
}
