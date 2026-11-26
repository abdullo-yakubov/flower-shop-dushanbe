using FlowerShop.Application.DTOs.Common;
using FlowerShop.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FlowerShop.API.Controllers;

[ApiController]
[Route("api/v1/delivery-zones")]
public class DeliveryZonesController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetZones(CancellationToken ct)
    {
        var zones = await db.DeliveryZones.Where(z => z.IsActive).OrderBy(z => z.SortOrder).ToListAsync(ct);
        var dtos = zones.Select(z => new { id = z.Id, nameRu = z.NameRu, fee = z.Fee, minFreeDelivery = z.MinFreeDelivery, minHours = z.MinHours, maxHours = z.MaxHours });
        return Ok(ApiResponse<object>.Ok(dtos));
    }
}
