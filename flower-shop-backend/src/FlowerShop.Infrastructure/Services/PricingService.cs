using FlowerShop.Application.DTOs.Orders;
using FlowerShop.Domain.Constants;
using FlowerShop.Domain.Entities;
using FlowerShop.Domain.Enums;
using FlowerShop.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FlowerShop.Infrastructure.Services;

/// <summary>A cart line resolved against the catalog, with the final per-unit price.</summary>
public sealed record PricedItem(
    Product Product,
    ProductSize Size,
    int Quantity,
    Packaging Packaging,
    int? StemCount,
    int UnitPrice)
{
    public int LineTotal => UnitPrice * Quantity;
    public string PackagingCode => Packaging == Packaging.Basket ? "basket" : "no_basket";
}

public sealed record PricedAddon(Addon Addon, int Quantity, string? Inscription)
{
    public int LineTotal => Addon.Price * Quantity;
}

/// <summary>
/// Full price breakdown. Invariant:
/// Total = ItemsSubtotal + AddonsTotal + DeliveryFee − PromoDiscount − PointsDiscount (never below 0).
/// </summary>
public sealed record PriceQuote(
    IReadOnlyList<PricedItem> Items,
    IReadOnlyList<PricedAddon> Addons,
    int ItemsSubtotal,
    int AddonsTotal,
    int DeliveryFee,
    PromoCode? Promo,
    int PromoDiscount,
    int PointsDiscount,
    int Total,
    int EarnPoints,
    string? HolidayName)
{
    public int Subtotal => ItemsSubtotal + AddonsTotal;
}

/// <summary>
/// Single source of truth for order pricing. The client-side calculation is only a preview;
/// the order is always charged with what this service returns.
/// </summary>
public class PricingService(AppDbContext db)
{
    public async Task<PriceQuote> QuoteAsync(Guid userId, OrderPricingInput input, CancellationToken ct)
    {
        var holiday = await FindHolidayAsync(input.DeliveryDate, ct);
        var items = await ResolveItemsAsync(input.Items, holiday, ct);
        var addons = await ResolveAddonsAsync(input.Addons, ct);

        var itemsSubtotal = items.Sum(i => i.LineTotal);
        var addonsTotal = addons.Sum(a => a.LineTotal);

        var deliveryFee = await CalculateDeliveryAsync(input.DeliveryZoneId, itemsSubtotal, ct);
        var (promo, promoDiscount) = await ResolvePromoAsync(input.PromoCode, itemsSubtotal, ct);

        var beforePoints = itemsSubtotal + addonsTotal + deliveryFee - promoDiscount;
        var pointsDiscount = await CalculatePointsDiscountAsync(userId, input.RedeemPoints, beforePoints, ct);

        var total = Math.Max(0, beforePoints - pointsDiscount);

        return new PriceQuote(items, addons, itemsSubtotal, addonsTotal, deliveryFee, promo, promoDiscount,
            pointsDiscount, total, total / LoyaltyConstants.EarnDivisor, holiday?.NameRu);
    }

    // ── Items ────────────────────────────────────────────────────────────────

    private async Task<List<PricedItem>> ResolveItemsAsync(
        IEnumerable<CreateOrderItem> requested, HolidayPricing? holiday, CancellationToken ct)
    {
        var lines = requested.ToList();
        if (lines.Count == 0)
            throw new AppException("VALIDATION_ERROR", "Заказ должен содержать минимум 1 товар");

        var sizeIds = lines.Select(l => l.SizeId).Distinct().ToList();
        var sizes = await db.ProductSizes
            .Include(s => s.Product).ThenInclude(p => p.Images)
            .Include(s => s.Product).ThenInclude(p => p.PackagingOptions)
            .Where(s => sizeIds.Contains(s.Id))
            .ToDictionaryAsync(s => s.Id, ct);

        var result = new List<PricedItem>(lines.Count);

        foreach (var line in lines)
        {
            if (!sizes.TryGetValue(line.SizeId, out var size))
                throw new AppException("NOT_FOUND", "Выбранный размер товара не найден");

            var product = size.Product;
            if (line.ProductId != Guid.Empty && line.ProductId != product.Id)
                throw new AppException("VALIDATION_ERROR", "Размер не относится к выбранному товару");
            if (!product.IsActive || !product.InStock)
                throw new AppException("OUT_OF_STOCK", $"Товар «{product.NameRu}» закончился");
            if (line.Quantity is < 1 or > OrderConstants.MaxLineQuantity)
                throw new AppException("VALIDATION_ERROR", $"Количество должно быть от 1 до {OrderConstants.MaxLineQuantity}");

            var packaging = ParsePackaging(line.Packaging);
            if (product.PackagingOptions.Count > 0 && product.PackagingOptions.All(p => p.Packaging != packaging))
                throw new AppException("VALIDATION_ERROR", $"Упаковка недоступна для товара «{product.NameRu}»");

            int basePrice;
            int? stemCount = null;
            if (product.Kind == ProductKind.Single && line.StemCount.HasValue && product.StemPrice is > 0)
            {
                if (line.StemCount <= 0 ||
                    (product.StemOptions.Length > 0 && !product.StemOptions.Contains(line.StemCount.Value)))
                    throw new AppException("VALIDATION_ERROR", "Недопустимое количество стеблей");

                stemCount = line.StemCount;
                basePrice = product.StemPrice.Value * stemCount.Value;
            }
            else
            {
                basePrice = size.Price;
            }

            if (holiday is not null && (holiday.AppliesTo == "all" || product.Freshness == Freshness.Live))
                basePrice = (int)Math.Round(basePrice * (1 + holiday.MarkupPercent / 100.0), MidpointRounding.AwayFromZero);

            var unitPrice = basePrice + (packaging == Packaging.Basket ? OrderConstants.BasketSurcharge : 0);
            result.Add(new PricedItem(product, size, line.Quantity, packaging, stemCount, unitPrice));
        }

        return result;
    }

    private static Packaging ParsePackaging(string? value) => value?.Trim().ToLowerInvariant() switch
    {
        "basket" => Packaging.Basket,
        null or "" or "no_basket" or "nobasket" => Packaging.NoBasket,
        _ => throw new AppException("VALIDATION_ERROR", "Недопустимый тип упаковки")
    };

    // ── Addons ───────────────────────────────────────────────────────────────

    private async Task<List<PricedAddon>> ResolveAddonsAsync(IEnumerable<CreateOrderAddon>? requested, CancellationToken ct)
    {
        var lines = requested?.ToList() ?? [];
        if (lines.Count == 0) return [];

        var ids = lines.Select(a => a.AddonId).Distinct().ToList();
        var addons = await db.Addons.Where(a => ids.Contains(a.Id) && a.IsActive).ToDictionaryAsync(a => a.Id, ct);

        return lines.Select(line =>
        {
            if (!addons.TryGetValue(line.AddonId, out var addon))
                throw new AppException("NOT_FOUND", "Дополнительный товар не найден или недоступен");
            if (line.Quantity is < 1 or > OrderConstants.MaxLineQuantity)
                throw new AppException("VALIDATION_ERROR", $"Количество должно быть от 1 до {OrderConstants.MaxLineQuantity}");

            var inscription = addon.HasInscription && !string.IsNullOrWhiteSpace(line.Inscription)
                ? line.Inscription.Trim()
                : null;
            if (inscription is { Length: > 100 })
                throw new AppException("VALIDATION_ERROR", "Надпись не длиннее 100 символов");

            return new PricedAddon(addon, line.Quantity, inscription);
        }).ToList();
    }

    // ── Delivery ─────────────────────────────────────────────────────────────

    private async Task<int> CalculateDeliveryAsync(Guid? zoneId, int itemsSubtotal, CancellationToken ct)
    {
        if (zoneId.HasValue)
        {
            var zone = await db.DeliveryZones.FirstOrDefaultAsync(z => z.Id == zoneId.Value && z.IsActive, ct)
                ?? throw new AppException("VALIDATION_ERROR", "Зона доставки не найдена");
            return itemsSubtotal >= zone.MinFreeDelivery ? 0 : zone.Fee;
        }

        var fee = await GetIntSettingAsync("default_delivery_fee", OrderConstants.DefaultDeliveryFee, ct);
        var threshold = await GetIntSettingAsync("free_delivery_threshold", OrderConstants.DefaultFreeDeliveryThreshold, ct);
        return itemsSubtotal >= threshold ? 0 : fee;
    }

    // ── Promo ────────────────────────────────────────────────────────────────

    /// <summary>Returns the active promo for <paramref name="code"/> or throws a user-facing error.</summary>
    public async Task<PromoCode> FindValidPromoAsync(string code, int itemsSubtotal, CancellationToken ct)
    {
        var normalized = code.Trim().ToUpperInvariant();
        var now = DateTimeOffset.UtcNow;
        var promo = await db.PromoCodes.FirstOrDefaultAsync(p =>
                p.Code == normalized && p.IsActive && p.ValidFrom <= now &&
                (p.ValidTo == null || p.ValidTo >= now) &&
                (p.MaxUses == null || p.UsedCount < p.MaxUses), ct)
            ?? throw new AppException("PROMO_INVALID", "Промокод не найден или недействителен");

        if (itemsSubtotal < promo.MinOrder)
            throw new AppException("PROMO_MIN_ORDER", $"Минимальная сумма заказа для промокода: {promo.MinOrder} с");

        return promo;
    }

    public static int CalculatePromoDiscount(PromoCode promo, int itemsSubtotal) =>
        promo.DiscountPercent > 0
            ? (int)Math.Round(itemsSubtotal * promo.DiscountPercent / 100.0, MidpointRounding.AwayFromZero)
            : Math.Min(promo.DiscountFixed, itemsSubtotal);

    private async Task<(PromoCode? promo, int discount)> ResolvePromoAsync(string? code, int itemsSubtotal, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(code)) return (null, 0);
        var promo = await FindValidPromoAsync(code, itemsSubtotal, ct);
        return (promo, CalculatePromoDiscount(promo, itemsSubtotal));
    }

    // ── Loyalty points ───────────────────────────────────────────────────────

    /// <summary>
    /// Points are worth 1 сомони each, capped by the balance and by MaxRedeemPercent of the amount due.
    /// Requests below MinRedeem are ignored.
    /// </summary>
    private async Task<int> CalculatePointsDiscountAsync(Guid userId, int requested, int amountDue, CancellationToken ct)
    {
        if (requested <= 0) return 0;
        if (requested < LoyaltyConstants.MinRedeem)
            throw new AppException("VALIDATION_ERROR", $"Минимум для списания: {LoyaltyConstants.MinRedeem} баллов");

        var balance = await db.LoyaltyAccounts.Where(a => a.UserId == userId).Select(a => a.Points).FirstOrDefaultAsync(ct);
        var cap = (int)Math.Floor(Math.Max(0, amountDue) * LoyaltyConstants.MaxRedeemPercent / 100.0);
        var applied = Math.Min(requested, Math.Min(balance, cap));

        return applied >= LoyaltyConstants.MinRedeem ? applied : 0;
    }

    // ── Helpers ──────────────────────────────────────────────────────────────

    private Task<HolidayPricing?> FindHolidayAsync(DateOnly date, CancellationToken ct) =>
        db.HolidayPricings.FirstOrDefaultAsync(h => h.Month == date.Month && h.Day == date.Day && h.IsActive, ct);

    private async Task<int> GetIntSettingAsync(string key, int fallback, CancellationToken ct)
    {
        var raw = await db.Settings.Where(s => s.Key == key).Select(s => s.Value).FirstOrDefaultAsync(ct);
        return int.TryParse(raw, out var value) ? value : fallback;
    }
}
