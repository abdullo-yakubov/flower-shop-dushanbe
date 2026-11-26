using FlowerShop.Application.DTOs.Common;
using FlowerShop.Application.DTOs.Orders;
using FlowerShop.Infrastructure.Services;
using Microsoft.AspNetCore.Mvc;

namespace FlowerShop.API.Controllers;

[ApiController]
[Route("api/v1/promo-codes")]
public class PromoCodesController(PricingService pricing) : ControllerBase
{
    /// <summary>Same rules as order pricing: dates, usage limit, minimum order amount.</summary>
    [HttpPost("validate")]
    public async Task<IActionResult> Validate([FromBody] PromoValidateRequest req, CancellationToken ct)
    {
        if (string.IsNullOrWhiteSpace(req.Code))
            return BadRequest(ApiResponse<object>.Fail("PROMO_INVALID", "Введите промокод"));

        var orderTotal = Math.Max(0, req.OrderTotal);
        var promo = await pricing.FindValidPromoAsync(req.Code, orderTotal, ct);
        var discount = PricingService.CalculatePromoDiscount(promo, orderTotal);

        return Ok(ApiResponse<PromoValidateResponse>.Ok(new PromoValidateResponse(
            promo.Code, promo.DiscountPercent, promo.DiscountFixed, discount, promo.LabelRu)));
    }
}
