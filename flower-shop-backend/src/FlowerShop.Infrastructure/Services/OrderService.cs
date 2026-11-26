using System.Text.Json;
using FlowerShop.Application.DTOs.Orders;
using FlowerShop.Domain.Entities;
using FlowerShop.Domain.Enums;
using FlowerShop.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FlowerShop.Infrastructure.Services;

public class OrderService(AppDbContext db, LoyaltyService loyalty, PricingService pricing)
{
    public async Task<CreateOrderResponse> CreateOrderAsync(Guid userId, CreateOrderRequest req, CancellationToken ct)
    {
        var user = await db.Users.FindAsync([userId], ct)
            ?? throw new AppException("NOT_FOUND", "Пользователь не найден");

        var items = new List<(ProductSize size, Product product, int quantity, string packaging, int? stemCount)>();
        foreach (var ri in req.Items)
        {
            var size = await db.ProductSizes
                .Include(s => s.Product).ThenInclude(p => p.Images)
                .FirstOrDefaultAsync(s => s.Id == ri.SizeId, ct)
                ?? throw new AppException("NOT_FOUND", $"Размер {ri.SizeId} не найден");
            if (!size.Product.InStock)
                throw new AppException("OUT_OF_STOCK", $"Товар {size.Product.NameRu} закончился");
            items.Add((size, size.Product, ri.Quantity, ri.Packaging, ri.StemCount));
        }

        var deliveryDate = DateOnly.Parse(req.DeliveryDate);
        var (subtotalItems, deliveryFee, promoDiscount, pointsDiscount, total) =
            await pricing.CalculateAsync(items, req.DeliveryZoneId, deliveryDate, req.PromoCode, req.RedeemPoints, userId, ct);

        int orderCount = await db.Orders.CountAsync(ct);
        var orderNumber = $"#{orderCount + 1:D6}";

        var order = new Order
        {
            Id = Guid.NewGuid(),
            OrderNumber = orderNumber,
            UserId = userId,
            Status = OrderStatus.Accepted,
            Subtotal = subtotalItems,
            DeliveryFee = deliveryFee,
            PromoDiscount = promoDiscount,
            PointsDiscount = pointsDiscount,
            Total = total,
            Address = req.Address,
            DeliveryZoneId = req.DeliveryZoneId,
            DeliveryDate = deliveryDate,
            DeliveryTime = req.DeliveryTime,
            PaymentMethod = Enum.Parse<PaymentMethod>(req.PaymentMethod, true),
            CustomerName = req.CustomerName,
            CustomerPhone = req.CustomerPhone,
            IsGift = req.IsGift,
            RecipientName = req.RecipientName,
            RecipientPhone = req.RecipientPhone,
            IsAnonymous = req.IsAnonymous,
            PostcardId = req.PostcardId,
            CardText = req.CardText,
            PromoCode = req.PromoCode,
            IsCancellable = true,
            CreatedAt = DateTimeOffset.UtcNow,
            UpdatedAt = DateTimeOffset.UtcNow
        };
        db.Orders.Add(order);

        foreach (var (size, product, qty, packaging, stemCount) in items)
        {
            var snapshot = JsonSerializer.SerializeToDocument(new { name = product.NameRu, image_url = product.Images.FirstOrDefault()?.Url, slug = product.Slug });
            db.OrderItems.Add(new OrderItem
            {
                OrderId = order.Id,
                ProductId = product.Id,
                ProductSnapshot = snapshot,
                SizeLabel = size.Label.ToString(),
                Quantity = qty,
                UnitPrice = size.Price,
                StemCount = stemCount,
                Packaging = packaging
            });
        }

        if (req.Addons is not null)
        {
            foreach (var ra in req.Addons)
            {
                var addon = await db.Addons.FindAsync([ra.AddonId], ct)
                    ?? throw new AppException("NOT_FOUND", $"Аддон {ra.AddonId} не найден");
                var snap = JsonSerializer.SerializeToDocument(new { name = addon.NameRu, price = addon.Price });
                db.OrderAddons.Add(new OrderAddon
                {
                    OrderId = order.Id,
                    AddonId = addon.Id,
                    AddonSnapshot = snap,
                    Quantity = ra.Quantity,
                    UnitPrice = addon.Price,
                    Inscription = ra.Inscription
                });
            }
        }

        db.OrderStatusHistories.Add(new OrderStatusHistory
        {
            OrderId = order.Id,
            ToStatus = OrderStatus.Accepted,
            CreatedAt = DateTimeOffset.UtcNow
        });

        if (req.PromoCode is not null)
        {
            var promo = await db.PromoCodes.FirstOrDefaultAsync(p => p.Code == req.PromoCode.ToUpper(), ct);
            if (promo is not null) promo.UsedCount++;
        }

        if (req.RedeemPoints > 0)
            await loyalty.RedeemPointsAsync(userId, req.RedeemPoints, order.Id, ct);

        user.TotalOrders++;
        user.TotalSpent += total;
        user.UpdatedAt = DateTimeOffset.UtcNow;

        await db.SaveChangesAsync(ct);

        var earned = await loyalty.EarnPointsAsync(userId, total, order.Id, ct);
        await db.SaveChangesAsync(ct);

        return new CreateOrderResponse(order.Id, orderNumber, total, earned, "accepted");
    }

    public async Task CancelOrderAsync(Guid orderId, Guid userId, string? reason, CancellationToken ct)
    {
        var order = await db.Orders.FindAsync([orderId], ct)
            ?? throw new AppException("NOT_FOUND", "Заказ не найден");
        if (order.UserId != userId)
            throw new AppException("FORBIDDEN", "Нет доступа к заказу");
        if (!order.IsCancellable || order.Status != OrderStatus.Accepted)
            throw new AppException("CONFLICT", "Заказ нельзя отменить");

        order.Status = OrderStatus.Cancelled;
        order.CancelReason = reason;
        order.UpdatedAt = DateTimeOffset.UtcNow;

        db.OrderStatusHistories.Add(new OrderStatusHistory
        {
            OrderId = order.Id,
            FromStatus = OrderStatus.Accepted,
            ToStatus = OrderStatus.Cancelled,
            ChangedBy = userId,
            Note = reason,
            CreatedAt = DateTimeOffset.UtcNow
        });

        await db.SaveChangesAsync(ct);
    }
}
