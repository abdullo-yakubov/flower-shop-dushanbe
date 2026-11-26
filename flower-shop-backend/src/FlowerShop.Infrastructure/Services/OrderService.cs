using System.Text.Json;
using FlowerShop.Application.DTOs.Orders;
using FlowerShop.Domain.Constants;
using FlowerShop.Domain.Entities;
using FlowerShop.Domain.Enums;
using FlowerShop.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FlowerShop.Infrastructure.Services;

public class OrderService(AppDbContext db, LoyaltyService loyalty, PricingService pricing)
{
    /// <summary>Arbitrary constant key for pg_advisory_xact_lock — serializes order numbering.</summary>
    private const long OrderNumberLockKey = 0x464C_5752_0001;

    public async Task<OrderQuoteResponse> QuoteAsync(Guid userId, QuoteOrderRequest req, CancellationToken ct)
    {
        var quote = await pricing.QuoteAsync(userId, new OrderPricingInput(
            req.Items, req.Addons, req.DeliveryZoneId, DateOnly.Parse(req.DeliveryDate), req.PromoCode, req.RedeemPoints), ct);

        return new OrderQuoteResponse(quote.ItemsSubtotal, quote.AddonsTotal, quote.DeliveryFee, quote.PromoDiscount,
            quote.PointsDiscount, quote.Total, quote.EarnPoints, quote.HolidayName);
    }

    public async Task<CreateOrderResponse> CreateOrderAsync(Guid userId, CreateOrderRequest req, CancellationToken ct)
    {
        var user = await db.Users.FindAsync([userId], ct)
            ?? throw new AppException("NOT_FOUND", "Пользователь не найден");
        if (user.IsBlocked)
            throw new AppException("FORBIDDEN", "Аккаунт заблокирован");

        if (req.PostcardId.HasValue && !await db.Postcards.AnyAsync(p => p.Id == req.PostcardId && p.IsActive, ct))
            throw new AppException("VALIDATION_ERROR", "Открытка не найдена");

        var deliveryDate = DateOnly.Parse(req.DeliveryDate);
        var deliveryTime = OrderConstants.NormalizeTimeSlot(req.DeliveryTime)
            ?? throw new AppException("VALIDATION_ERROR", "Недопустимый временной слот");

        await using var tx = await db.Database.BeginTransactionAsync(ct);
        await db.Database.ExecuteSqlRawAsync("SELECT pg_advisory_xact_lock({0})", [OrderNumberLockKey], ct);

        // Priced inside the transaction so promo limits and the points balance can't change underneath us.
        var quote = await pricing.QuoteAsync(userId, new OrderPricingInput(
            req.Items, req.Addons, req.DeliveryZoneId, deliveryDate, req.PromoCode, req.RedeemPoints), ct);

        if (req.ExpectedTotal is int expectedTotal && expectedTotal != quote.Total)
            throw new AppException("PRICE_CHANGED", "Итог заказа изменился. Проверьте обновлённую сумму.");

        var now = DateTimeOffset.UtcNow;
        var order = new Order
        {
            Id = Guid.NewGuid(),
            OrderNumber = await NextOrderNumberAsync(ct),
            UserId = userId,
            Status = OrderStatus.Accepted,
            Subtotal = quote.Subtotal,
            DeliveryFee = quote.DeliveryFee,
            PromoDiscount = quote.PromoDiscount,
            PointsDiscount = quote.PointsDiscount,
            Total = quote.Total,
            Address = req.Address.Trim(),
            DeliveryZoneId = req.DeliveryZoneId,
            DeliveryDate = deliveryDate,
            DeliveryTime = deliveryTime,
            PaymentMethod = Enum.Parse<PaymentMethod>(req.PaymentMethod, ignoreCase: true),
            CustomerName = req.CustomerName.Trim(),
            CustomerPhone = req.CustomerPhone,
            IsGift = req.IsGift,
            RecipientName = req.IsGift ? req.RecipientName?.Trim() : null,
            RecipientPhone = req.IsGift ? req.RecipientPhone : null,
            IsAnonymous = req.IsAnonymous,
            PostcardId = req.PostcardId,
            CardText = string.IsNullOrWhiteSpace(req.CardText) ? null : req.CardText.Trim(),
            PromoCode = quote.Promo?.Code,
            IsCancellable = true,
            CreatedAt = now,
            UpdatedAt = now
        };
        db.Orders.Add(order);

        foreach (var item in quote.Items)
        {
            db.OrderItems.Add(new OrderItem
            {
                OrderId = order.Id,
                ProductId = item.Product.Id,
                ProductSnapshot = JsonSerializer.SerializeToDocument(new
                {
                    name = item.Product.NameRu,
                    image_url = item.Product.Images.OrderBy(i => i.SortOrder).FirstOrDefault()?.Url,
                    slug = item.Product.Slug,
                    unit_price = item.UnitPrice
                }),
                SizeLabel = item.Size.Label.ToString(),
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice,
                StemCount = item.StemCount,
                Packaging = item.PackagingCode
            });
        }

        foreach (var addon in quote.Addons)
        {
            db.OrderAddons.Add(new OrderAddon
            {
                OrderId = order.Id,
                AddonId = addon.Addon.Id,
                AddonSnapshot = JsonSerializer.SerializeToDocument(new { name = addon.Addon.NameRu, price = addon.Addon.Price }),
                Quantity = addon.Quantity,
                UnitPrice = addon.Addon.Price,
                Inscription = addon.Inscription
            });
        }

        db.OrderStatusHistories.Add(new OrderStatusHistory
        {
            OrderId = order.Id,
            ToStatus = OrderStatus.Accepted,
            CreatedAt = now
        });

        if (quote.Promo is not null)
            quote.Promo.UsedCount++;

        await loyalty.RedeemPointsAsync(userId, quote.PointsDiscount, order, ct);
        var earned = await loyalty.EarnPointsAsync(userId, quote.Total, order, ct);

        user.TotalOrders++;
        user.TotalSpent += quote.Total;
        user.UpdatedAt = now;

        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);

        return new CreateOrderResponse(order.Id, order.OrderNumber, order.Total, earned, order.Status.ToApiString());
    }

    public async Task CancelOrderAsync(Guid orderId, Guid userId, string? reason, CancellationToken ct)
    {
        await using var tx = await db.Database.BeginTransactionAsync(ct);

        var order = await db.Orders.FirstOrDefaultAsync(o => o.Id == orderId, ct)
            ?? throw new AppException("NOT_FOUND", "Заказ не найден");
        if (order.UserId != userId)
            throw new AppException("FORBIDDEN", "Нет доступа к заказу");
        if (!order.IsCancellable || order.Status != OrderStatus.Accepted)
            throw new AppException("CONFLICT", "Заказ уже передан в работу и не может быть отменён");

        var now = DateTimeOffset.UtcNow;
        var note = string.IsNullOrWhiteSpace(reason) ? null : reason.Trim()[..Math.Min(reason.Trim().Length, 255)];

        order.Status = OrderStatus.Cancelled;
        order.IsCancellable = false;
        order.CancelReason = note;
        order.UpdatedAt = now;

        db.OrderStatusHistories.Add(new OrderStatusHistory
        {
            OrderId = order.Id,
            FromStatus = OrderStatus.Accepted,
            ToStatus = OrderStatus.Cancelled,
            ChangedBy = userId,
            Note = note,
            CreatedAt = now
        });

        await loyalty.RevertOrderAsync(userId, order, ct);

        if (order.PromoCode is not null)
        {
            var promo = await db.PromoCodes.FirstOrDefaultAsync(p => p.Code == order.PromoCode, ct);
            if (promo is { UsedCount: > 0 }) promo.UsedCount--;
        }

        var user = await db.Users.FindAsync([userId], ct);
        if (user is not null)
        {
            user.TotalOrders = Math.Max(0, user.TotalOrders - 1);
            user.TotalSpent = Math.Max(0, user.TotalSpent - order.Total);
            user.UpdatedAt = now;
        }

        await db.SaveChangesAsync(ct);
        await tx.CommitAsync(ct);
    }

    /// <summary>Must be called while holding the advisory lock. Numbers look like "#000123".</summary>
    private async Task<string> NextOrderNumberAsync(CancellationToken ct)
    {
        var last = await db.Orders
            .Where(o => o.OrderNumber.StartsWith(OrderConstants.NumberPrefix))
            .OrderByDescending(o => o.OrderNumber.Length).ThenByDescending(o => o.OrderNumber)
            .Select(o => o.OrderNumber)
            .FirstOrDefaultAsync(ct);

        var lastValue = last is not null && int.TryParse(last[OrderConstants.NumberPrefix.Length..], out var n) ? n : 0;
        return $"{OrderConstants.NumberPrefix}{lastValue + 1:D6}";
    }
}
