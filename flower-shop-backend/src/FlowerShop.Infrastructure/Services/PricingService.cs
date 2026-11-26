using FlowerShop.Domain.Constants;
using FlowerShop.Domain.Entities;
using FlowerShop.Domain.Enums;
using FlowerShop.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FlowerShop.Infrastructure.Services;

public class PricingService(AppDbContext db)
{
    public async Task<(int subtotal, int deliveryFee, int promoDiscount, int pointsDiscount, int total)>
        CalculateAsync(
            IEnumerable<(ProductSize size, Product product, int qty, string packaging, int? stemCount)> items,
            Guid? zoneId, DateOnly deliveryDate, string? promoCode, int redeemPoints,
            Guid userId, CancellationToken ct)
    {
        var holiday = await db.HolidayPricings
            .FirstOrDefaultAsync(h => h.Month == deliveryDate.Month && h.Day == deliveryDate.Day && h.IsActive, ct);

        int subtotal = 0;
        foreach (var (size, product, qty, _, stemCount) in items)
        {
            var unitPrice = size.Price;
            if (holiday is not null && (holiday.AppliesTo == "all" || product.Freshness == Freshness.Live))
                unitPrice = (int)Math.Round(unitPrice * (1 + holiday.MarkupPercent / 100.0));
            subtotal += unitPrice * qty;
        }

        int deliveryFee = 0;
        if (zoneId.HasValue)
        {
            var zone = await db.DeliveryZones.FindAsync([zoneId.Value], ct);
            if (zone is not null)
                deliveryFee = subtotal >= zone.MinFreeDelivery ? 0 : zone.Fee;
        }

        int promoDiscount = 0;
        if (!string.IsNullOrWhiteSpace(promoCode))
        {
            var promo = await db.PromoCodes
                .FirstOrDefaultAsync(p => p.Code == promoCode.ToUpper() && p.IsActive && p.ValidFrom <= DateTimeOffset.UtcNow
                    && (p.ValidTo == null || p.ValidTo >= DateTimeOffset.UtcNow)
                    && (p.MaxUses == null || p.UsedCount < p.MaxUses), ct);

            if (promo is not null && subtotal >= promo.MinOrder)
                promoDiscount = promo.DiscountPercent > 0
                    ? (int)Math.Round(subtotal * promo.DiscountPercent / 100.0)
                    : promo.DiscountFixed;
        }

        int pointsDiscount = 0;
        if (redeemPoints >= LoyaltyConstants.MinRedeem)
        {
            var account = await db.LoyaltyAccounts.FirstOrDefaultAsync(a => a.UserId == userId, ct);
            var maxRedeem = Math.Min(account?.Points ?? 0, (int)Math.Floor(subtotal * LoyaltyConstants.MaxRedeemPercent / 100.0));
            pointsDiscount = Math.Min(redeemPoints, maxRedeem);
        }

        var total = Math.Max(0, subtotal + deliveryFee - promoDiscount - pointsDiscount);
        return (subtotal, deliveryFee, promoDiscount, pointsDiscount, total);
    }
}
