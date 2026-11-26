using FlowerShop.Application.DTOs.Common;
using FlowerShop.Application.DTOs.Orders;
using FlowerShop.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FlowerShop.API.Controllers;

[ApiController]
[Route("api/v1/promo-codes")]
public class PromoCodesController(AppDbContext db) : ControllerBase
{
    [HttpPost("validate")]
    public async Task<IActionResult> Validate([FromBody] PromoValidateRequest req, CancellationToken ct)
    {
        var promo = await db.PromoCodes.FirstOrDefaultAsync(p =>
            p.Code == req.Code.ToUpper() && p.IsActive &&
            p.ValidFrom <= DateTimeOffset.UtcNow &&
            (p.ValidTo == null || p.ValidTo >= DateTimeOffset.UtcNow) &&
            (p.MaxUses == null || p.UsedCount < p.MaxUses), ct);

        if (promo is null)
            return BadRequest(ApiResponse<object>.Fail("PROMO_INVALID", "Промокод не найден или недействителен"));

        if (req.OrderTotal < promo.MinOrder)
            return BadRequest(ApiResponse<object>.Fail("PROMO_MIN_ORDER", $"Минимальная сумма заказа: {promo.MinOrder} с"));

        var discountAmount = promo.DiscountPercent > 0
            ? (int)Math.Round(req.OrderTotal * promo.DiscountPercent / 100.0)
            : promo.DiscountFixed;

        return Ok(ApiResponse<PromoValidateResponse>.Ok(new PromoValidateResponse(
            promo.Code, promo.DiscountPercent, promo.DiscountFixed, discountAmount, promo.LabelRu)));
    }
}
