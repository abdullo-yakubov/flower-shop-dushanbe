using FlowerShop.Application.DTOs.Common;
using FlowerShop.Domain.Constants;
using FlowerShop.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FlowerShop.API.Controllers;

[ApiController]
[Route("api/v1/config")]
public class ConfigController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetConfig(CancellationToken ct)
    {
        var settings = await db.Settings.ToListAsync(ct);
        var s = settings.ToDictionary(x => x.Key, x => x.Value);

        var holidays = await db.HolidayPricings.Where(h => h.IsActive).ToListAsync(ct);

        return Ok(ApiResponse<object>.Ok(new
        {
            shopName = s.GetValueOrDefault("shop_name", "Flower Shop"),
            shopPhone = s.GetValueOrDefault("shop_phone"),
            shopTelegram = s.GetValueOrDefault("shop_telegram"),
            shopInstagram = s.GetValueOrDefault("shop_instagram"),
            openTime = s.GetValueOrDefault("shop_open_time", "06:00"),
            closeTime = s.GetValueOrDefault("shop_close_time", "00:00"),
            defaultDeliveryFee = int.TryParse(s.GetValueOrDefault("default_delivery_fee"), out var df) ? df : 40,
            freeDeliveryThreshold = int.TryParse(s.GetValueOrDefault("free_delivery_threshold"), out var fdt) ? fdt : 1500,
            loyaltyEarnDivisor = int.TryParse(s.GetValueOrDefault("loyalty_earn_divisor"), out var ed) ? ed : 20,
            loyaltyMinRedeem = int.TryParse(s.GetValueOrDefault("loyalty_min_redeem"), out var mr) ? mr : 50,
            loyaltyMaxRedeemPct = int.TryParse(s.GetValueOrDefault("loyalty_max_redeem_pct"), out var mrp) ? mrp : 30,
            holidayPricing = holidays.Select(h => new { month = h.Month, day = h.Day, nameRu = h.NameRu, markupPercent = h.MarkupPercent }),
            timeSlots = OrderConstants.TimeSlots
        }));
    }
}
