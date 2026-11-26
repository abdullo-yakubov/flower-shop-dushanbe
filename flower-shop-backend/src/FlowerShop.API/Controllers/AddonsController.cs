using FlowerShop.Application.DTOs.Common;
using FlowerShop.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FlowerShop.API.Controllers;

[ApiController]
[Route("api/v1/addons")]
public class AddonsController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAddons([FromQuery] string lang = "ru", CancellationToken ct = default)
    {
        var addons = await db.Addons.Where(a => a.IsActive).OrderBy(a => a.SortOrder).ToListAsync(ct);
        var dtos = addons.Select(a => new
        {
            id = a.Id,
            type = a.Type,
            name = lang == "en" ? a.NameEn : lang == "tg" ? a.NameTg : a.NameRu,
            imageUrl = a.ImageUrl,
            price = a.Price,
            hasInscription = a.HasInscription
        });
        return Ok(ApiResponse<object>.Ok(dtos));
    }
}
