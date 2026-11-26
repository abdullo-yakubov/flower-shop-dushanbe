using System.Security.Claims;
using System.Text.Json;
using FlowerShop.Application.DTOs.Common;
using FlowerShop.Application.DTOs.Orders;
using FlowerShop.Domain.Constants;
using FlowerShop.Domain.Enums;
using FlowerShop.Infrastructure.Persistence;
using FlowerShop.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FlowerShop.API.Controllers;

[ApiController]
[Route("api/v1/orders")]
[Authorize]
public class OrdersController(AppDbContext db, OrderService orderService) : ControllerBase
{
    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    /// <summary>Server-side price preview — the same calculation that <see cref="Create"/> charges.</summary>
    [HttpPost("quote")]
    public async Task<IActionResult> Quote([FromBody] QuoteOrderRequest req, CancellationToken ct)
    {
        var result = await orderService.QuoteAsync(UserId, req, ct);
        return Ok(ApiResponse<OrderQuoteResponse>.Ok(result));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateOrderRequest req, CancellationToken ct)
    {
        var result = await orderService.CreateOrderAsync(UserId, req, ct);
        return StatusCode(StatusCodes.Status201Created, ApiResponse<CreateOrderResponse>.Ok(result));
    }

    [HttpGet]
    public async Task<IActionResult> GetMyOrders(
        [FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? status = null,
        CancellationToken ct = default)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 50);

        var userId = UserId;
        var query = db.Orders.AsNoTracking().Include(o => o.Items).Where(o => o.UserId == userId);
        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<OrderStatus>(status.Replace("_", ""), true, out var s))
            query = query.Where(o => o.Status == s);

        var total = await query.CountAsync(ct);
        var orders = await query.OrderByDescending(o => o.CreatedAt)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);

        var dtos = orders.Select(o => new OrderListItemDto(
            o.Id, o.OrderNumber, o.Status.ToApiString(), o.Total,
            o.DeliveryDate.ToString("yyyy-MM-dd"), o.DeliveryTime,
            o.Items.Sum(i => i.Quantity), FirstImage(o.Items), o.IsCancellable, o.CreatedAt));

        return Ok(PagedResponse<OrderListItemDto>.Ok(dtos, page, pageSize, total));
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetOrder(Guid id, CancellationToken ct)
    {
        var userId = UserId;
        var order = await db.Orders.AsNoTracking()
            .Include(o => o.Items)
            .Include(o => o.Addons)
            .Include(o => o.StatusHistory)
            .FirstOrDefaultAsync(o => o.Id == id && o.UserId == userId, ct);

        if (order is null) return NotFound(ApiResponse<object>.Fail("NOT_FOUND", "Заказ не найден"));

        return Ok(ApiResponse<object>.Ok(new
        {
            id = order.Id,
            orderNumber = order.OrderNumber,
            status = order.Status.ToApiString(),
            total = order.Total,
            subtotal = order.Subtotal,
            deliveryFee = order.DeliveryFee,
            promoDiscount = order.PromoDiscount,
            pointsDiscount = order.PointsDiscount,
            address = order.Address,
            deliveryDate = order.DeliveryDate.ToString("yyyy-MM-dd"),
            deliveryTime = order.DeliveryTime,
            paymentMethod = order.PaymentMethod.ToApiString(),
            isPaid = order.IsPaid,
            isCancellable = order.IsCancellable,
            isGift = order.IsGift,
            customerName = order.CustomerName,
            customerPhone = order.CustomerPhone,
            recipientName = order.RecipientName,
            recipientPhone = order.RecipientPhone,
            cardText = order.CardText,
            promoCode = order.PromoCode,
            itemCount = order.Items.Sum(i => i.Quantity),
            firstItemImage = FirstImage(order.Items),
            createdAt = order.CreatedAt,
            updatedAt = order.UpdatedAt,
            items = order.Items.Select(i => new
            {
                id = i.Id,
                productId = i.ProductId,
                productName = SnapshotString(i.ProductSnapshot, "name"),
                imageUrl = SnapshotString(i.ProductSnapshot, "image_url"),
                sizeLabel = i.SizeLabel,
                quantity = i.Quantity,
                unitPrice = i.UnitPrice,
                stemCount = i.StemCount,
                packaging = i.Packaging
            }),
            addons = order.Addons.Select(a => new
            {
                id = a.Id,
                addonId = a.AddonId,
                name = SnapshotString(a.AddonSnapshot, "name"),
                quantity = a.Quantity,
                unitPrice = a.UnitPrice,
                inscription = a.Inscription
            }),
            statusHistory = order.StatusHistory.OrderBy(h => h.CreatedAt).Select(h => new
            {
                status = h.ToStatus.ToApiString(),
                fromStatus = h.FromStatus?.ToApiString(),
                note = h.Note,
                photoUrl = h.PhotoUrl,
                createdAt = h.CreatedAt
            })
        }));
    }

    [HttpPost("{id:guid}/cancel")]
    public async Task<IActionResult> Cancel(Guid id, [FromBody] CancelOrderRequest? req, CancellationToken ct)
    {
        await orderService.CancelOrderAsync(id, UserId, req?.Reason, ct);
        return Ok(new { success = true });
    }

    private static string? FirstImage(IEnumerable<Domain.Entities.OrderItem> items) =>
        items.Select(i => SnapshotString(i.ProductSnapshot, "image_url")).FirstOrDefault(u => u is not null);

    private static string? SnapshotString(JsonDocument? doc, string property) =>
        doc?.RootElement.ValueKind == JsonValueKind.Object &&
        doc.RootElement.TryGetProperty(property, out var value) &&
        value.ValueKind == JsonValueKind.String
            ? value.GetString()
            : null;
}
