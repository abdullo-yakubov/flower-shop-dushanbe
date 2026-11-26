using FlowerShop.Application.DTOs.Orders;
using FlowerShop.Infrastructure.Services;

namespace FlowerShop.Tests;

public class PricingServiceTests
{
    // A regular weekday far from seeded holidays.
    private static readonly DateOnly Date = new(2030, 6, 11);

    private static OrderPricingInput Input(
        IEnumerable<CreateOrderItem> items, IEnumerable<CreateOrderAddon>? addons = null,
        string? promo = null, int points = 0, DateOnly? date = null) =>
        new(items, addons, null, date ?? Date, promo, points);

    [Fact]
    public async Task Bouquet_without_extras_adds_default_delivery()
    {
        using var t = new TestDb();
        var quote = await new PricingService(t.Db).QuoteAsync(t.UserId,
            Input([new CreateOrderItem(t.Bouquet.Id, t.BouquetM.Id, 2)]), default);

        Assert.Equal(1200, quote.ItemsSubtotal);
        Assert.Equal(40, quote.DeliveryFee);
        Assert.Equal(1240, quote.Total);
        Assert.Equal(1240 / 20, quote.EarnPoints);
    }

    [Fact]
    public async Task Free_delivery_from_threshold()
    {
        using var t = new TestDb();
        var quote = await new PricingService(t.Db).QuoteAsync(t.UserId,
            Input([new CreateOrderItem(t.Bouquet.Id, t.BouquetM.Id, 3)]), default);

        Assert.Equal(1800, quote.ItemsSubtotal);
        Assert.Equal(0, quote.DeliveryFee);
    }

    [Fact]
    public async Task Basket_surcharge_and_addons_are_charged()
    {
        using var t = new TestDb();
        var quote = await new PricingService(t.Db).QuoteAsync(t.UserId,
            Input([new CreateOrderItem(t.Bouquet.Id, t.BouquetM.Id, 1, "basket")],
                [new CreateOrderAddon(t.Chocolate.Id, 2)]), default);

        Assert.Equal(680, quote.ItemsSubtotal);   // 600 + 80 basket
        Assert.Equal(180, quote.AddonsTotal);     // 2 × 90
        Assert.Equal(680 + 180 + 40, quote.Total);
    }

    [Fact]
    public async Task Single_stems_are_priced_per_stem()
    {
        using var t = new TestDb();
        var quote = await new PricingService(t.Db).QuoteAsync(t.UserId,
            Input([new CreateOrderItem(t.Tulips.Id, t.TulipsSize.Id, 1, "no_basket", 25)]), default);

        Assert.Equal(300, quote.ItemsSubtotal);   // 25 × 12
        Assert.Equal(25, quote.Items[0].StemCount);
    }

    [Fact]
    public async Task Stem_count_outside_options_is_rejected()
    {
        using var t = new TestDb();
        var ex = await Assert.ThrowsAsync<AppException>(() => new PricingService(t.Db).QuoteAsync(t.UserId,
            Input([new CreateOrderItem(t.Tulips.Id, t.TulipsSize.Id, 1, "no_basket", 7)]), default));
        Assert.Equal("VALIDATION_ERROR", ex.Code);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-3)]
    [InlineData(21)]
    public async Task Invalid_quantity_is_rejected(int quantity)
    {
        using var t = new TestDb();
        await Assert.ThrowsAsync<AppException>(() => new PricingService(t.Db).QuoteAsync(t.UserId,
            Input([new CreateOrderItem(t.Bouquet.Id, t.BouquetM.Id, quantity)]), default));
    }

    [Fact]
    public async Task Size_from_another_product_is_rejected()
    {
        using var t = new TestDb();
        var ex = await Assert.ThrowsAsync<AppException>(() => new PricingService(t.Db).QuoteAsync(t.UserId,
            Input([new CreateOrderItem(t.Tulips.Id, t.BouquetM.Id)]), default));
        Assert.Equal("VALIDATION_ERROR", ex.Code);
    }

    [Fact]
    public async Task Unavailable_packaging_is_rejected()
    {
        using var t = new TestDb();
        await Assert.ThrowsAsync<AppException>(() => new PricingService(t.Db).QuoteAsync(t.UserId,
            Input([new CreateOrderItem(t.Dried.Id, t.DriedSize.Id, 1, "basket")]), default));
    }

    [Fact]
    public async Task Out_of_stock_is_rejected()
    {
        using var t = new TestDb();
        t.Bouquet.InStock = false;
        await t.Db.SaveChangesAsync();

        var ex = await Assert.ThrowsAsync<AppException>(() => new PricingService(t.Db).QuoteAsync(t.UserId,
            Input([new CreateOrderItem(t.Bouquet.Id, t.BouquetM.Id)]), default));
        Assert.Equal("OUT_OF_STOCK", ex.Code);
    }

    [Fact]
    public async Task Holiday_markup_applies_to_live_flowers_only()
    {
        using var t = new TestDb();
        t.AddHoliday(Date, 30);
        var quote = await new PricingService(t.Db).QuoteAsync(t.UserId,
            Input([new CreateOrderItem(t.Bouquet.Id, t.BouquetM.Id, 1, "basket"),
                   new CreateOrderItem(t.Dried.Id, t.DriedSize.Id)]), default);

        Assert.Equal(600 * 13 / 10 + 80, quote.Items[0].UnitPrice); // markup not applied to the basket
        Assert.Equal(400, quote.Items[1].UnitPrice);
        Assert.Equal("Праздник", quote.HolidayName);
    }

    [Fact]
    public async Task Percent_promo_applies_to_items()
    {
        using var t = new TestDb();
        var quote = await new PricingService(t.Db).QuoteAsync(t.UserId,
            Input([new CreateOrderItem(t.Bouquet.Id, t.BouquetM.Id)], promo: " flower10 "), default);

        Assert.Equal(60, quote.PromoDiscount);
        Assert.Equal(600 + 40 - 60, quote.Total);
        Assert.Same(t.Percent10, quote.Promo);
    }

    [Fact]
    public async Task Fixed_promo_never_exceeds_items_subtotal()
    {
        using var t = new TestDb();
        var quote = await new PricingService(t.Db).QuoteAsync(t.UserId,
            Input([new CreateOrderItem(t.Dried.Id, t.DriedSize.Id)], promo: "MINUS500"), default);

        Assert.Equal(400, quote.PromoDiscount);
        Assert.Equal(40, quote.Total);
    }

    [Fact]
    public async Task Exhausted_or_unknown_promo_is_rejected()
    {
        using var t = new TestDb();
        t.Percent10.MaxUses = 1;
        t.Percent10.UsedCount = 1;
        await t.Db.SaveChangesAsync();

        var pricing = new PricingService(t.Db);
        var items = new[] { new CreateOrderItem(t.Bouquet.Id, t.BouquetM.Id) };
        Assert.Equal("PROMO_INVALID",
            (await Assert.ThrowsAsync<AppException>(() => pricing.QuoteAsync(t.UserId, Input(items, promo: "FLOWER10"), default))).Code);
        Assert.Equal("PROMO_INVALID",
            (await Assert.ThrowsAsync<AppException>(() => pricing.QuoteAsync(t.UserId, Input(items, promo: "NOPE"), default))).Code);
    }

    [Fact]
    public async Task Promo_minimum_order_is_enforced()
    {
        using var t = new TestDb();
        t.Percent10.MinOrder = 1000;
        await t.Db.SaveChangesAsync();

        var ex = await Assert.ThrowsAsync<AppException>(() => new PricingService(t.Db).QuoteAsync(t.UserId,
            Input([new CreateOrderItem(t.Bouquet.Id, t.BouquetM.Id)], promo: "FLOWER10"), default));
        Assert.Equal("PROMO_MIN_ORDER", ex.Code);
    }

    [Fact]
    public async Task Points_are_capped_by_percent_of_amount_due()
    {
        using var t = new TestDb();
        t.SetPoints(1000);
        var quote = await new PricingService(t.Db).QuoteAsync(t.UserId,
            Input([new CreateOrderItem(t.Bouquet.Id, t.BouquetM.Id)], points: 1000), default);

        Assert.Equal((600 + 40) * 30 / 100, quote.PointsDiscount); // 192
        Assert.Equal(640 - 192, quote.Total);
    }

    [Fact]
    public async Task Points_are_capped_by_balance()
    {
        using var t = new TestDb();
        t.SetPoints(70);
        var quote = await new PricingService(t.Db).QuoteAsync(t.UserId,
            Input([new CreateOrderItem(t.Bouquet.Id, t.BouquetM.Id)], points: 150), default);

        Assert.Equal(70, quote.PointsDiscount);
    }

    [Fact]
    public async Task Points_below_minimum_are_rejected()
    {
        using var t = new TestDb();
        t.SetPoints(500);
        await Assert.ThrowsAsync<AppException>(() => new PricingService(t.Db).QuoteAsync(t.UserId,
            Input([new CreateOrderItem(t.Bouquet.Id, t.BouquetM.Id)], points: 20), default));
    }
}
