using FlowerShop.Application.DTOs.Common;
using FlowerShop.Application.DTOs.Orders;
using FlowerShop.Infrastructure.Services;
using FlowerShop.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FlowerShop.API.Controllers;

[ApiController]
[Route("api/v1/orders")]
[Authorize]
public class OrdersController(AppDbContext db, OrderService orderService) : ControllerBase
{
    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateOrderRequest req, CancellationToken ct)
    {
        var result = await orderService.CreateOrderAsync(UserId, req, ct);
        return StatusCode(201, ApiResponse<CreateOrderResponse>.Ok(result));
    }

    [HttpGet]
    public async Task<IActionResult> GetMyOrders([FromQuery] int page = 1, [FromQuery] int pageSize = 10, [FromQuery] string? status = null, CancellationToken ct = default)
    {
        var query = db.Orders.Include(o => o.Items).Where(o => o.UserId == UserId);
        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<Domain.Enums.OrderStatus>(status, true, out var s))
            query = query.Where(o => o.Status == s);

        var total = await query.CountAsync(ct);
        var orders = await query.OrderByDescending(o => o.CreatedAt)
            .Skip((page - 1) * pageSize).Take(pageSize).ToListAsync(ct);

        var dtos = orders.Select(o => new OrderListItemDto(
            o.Id, o.OrderNumber, o.Status.ToString().ToLower(), o.Total,
            o.DeliveryDate.ToString("yyyy-MM-dd"), o.DeliveryTime,
            o.Items.Sum(i => i.Quantity), null, o.IsCancellable, o.CreatedAt
        ));

        return Ok(PagedResponse<OrderListItemDto>.Ok(dtos, page, pageSize, total));
    }

    [HttpGet("{id}")]
    public async Task<IActionResult> GetOrder(Guid id, CancellationToken ct)
    {
        var order = await db.Orders.Include(o => o.Items).ThenInclude(i => i.Product)
            .Include(o => o.Addons).ThenInclude(a => a.Addon)
            .Include(o => o.StatusHistory)
            .FirstOrDefaultAsync(o => o.Id == id && o.UserId == UserId, ct);

        if (order is null) return NotFound(ApiResponse<object>.Fail("NOT_FOUND", "Заказ не найден"));
        return Ok(ApiResponse<object>.Ok(new
        {
            id = order.Id, orderNumber = order.OrderNumber, status = order.Status.ToString().ToLower(),
            total = order.Total, subtotal = order.Subtotal, deliveryFee = order.DeliveryFee,
            promoDiscount = order.PromoDiscount, pointsDiscount = order.PointsDiscount,
            address = order.Address, deliveryDate = order.DeliveryDate.ToString("yyyy-MM-dd"), deliveryTime = order.DeliveryTime,
            paymentMethod = order.PaymentMethod.ToString().ToLower(), isPaid = order.IsPaid, isCancellable = order.IsCancellable,
            isGift = order.IsGift, customerName = order.CustomerName, customerPhone = order.CustomerPhone,
            recipientName = order.RecipientName, recipientPhone = order.RecipientPhone, cardText = order.CardText,
            promoCode = order.PromoCode, createdAt = order.CreatedAt, updatedAt = order.UpdatedAt,
            items = order.Items.Select(i => new
            {
                id = i.Id, productId = i.ProductId, productName = i.ProductSnapshot?.RootElement.TryGetProperty("name", out var n) == true ? n.GetString() : null,
                sizeLabel = i.SizeLabel, quantity = i.Quantity, unitPrice = i.UnitPrice, packaging = i.Packaging
            }),
            addons = order.Addons.Select(a => new { id = a.Id, addonId = a.AddonId, quantity = a.Quantity, unitPrice = a.UnitPrice }),
            statusHistory = order.StatusHistory.OrderBy(h => h.CreatedAt).Select(h => new
            {
                status = h.ToStatus.ToString().ToLower(),
                fromStatus = h.FromStatus.HasValue ? h.FromStatus.Value.ToString().ToLower() : null,
                note = h.Note,
                photoUrl = h.PhotoUrl,
                createdAt = h.CreatedAt
            })
        }));
    }

    [HttpPost("{id}/cancel")]
    public async Task<IActionResult> Cancel(Guid id, [FromBody] CancelOrderRequest req, CancellationToken ct)
    {
        await orderService.CancelOrderAsync(id, UserId, req.Reason, ct);
        return Ok(new { success = true });
    }
}
