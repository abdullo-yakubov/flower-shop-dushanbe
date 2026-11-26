using FlowerShop.Application.DTOs.Common;
using FlowerShop.Infrastructure.Persistence;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace FlowerShop.API.Controllers;

[ApiController]
[Route("api/v1")]
public class PostcardsController(AppDbContext db) : ControllerBase
{
    [HttpGet("postcards")]
    public async Task<IActionResult> GetPostcards([FromQuery] string? occasion = null, [FromQuery] string lang = "ru", CancellationToken ct = default)
    {
        var query = db.Postcards.Where(p => p.IsActive);
        if (!string.IsNullOrWhiteSpace(occasion) && occasion != "all")
            query = query.Where(p => p.Occasion == occasion);

        var items = await query.OrderBy(p => p.SortOrder).ToListAsync(ct);
        var dtos = items.Select(p => new
        {
            id = p.Id,
            occasion = p.Occasion,
            imageUrl = p.ImageUrl,
            emoji = p.Emoji,
            name = lang == "en" ? p.NameEn : lang == "tg" ? p.NameTg : p.NameRu,
            isPopular = p.IsPopular
        });
        return Ok(ApiResponse<object>.Ok(dtos));
    }

    [HttpGet("message-templates")]
    public async Task<IActionResult> GetMessageTemplates([FromQuery] string? category = null, [FromQuery] string lang = "ru", CancellationToken ct = default)
    {
        var query = db.MessageTemplates.Where(t => t.IsActive);
        if (!string.IsNullOrWhiteSpace(category) && category != "all")
            query = query.Where(t => t.Category == category);

        var items = await query.OrderBy(t => t.SortOrder).ToListAsync(ct);
        var dtos = items.Select(t => new
        {
            id = t.Id,
            category = t.Category,
            text = lang == "en" ? t.TextEn : lang == "tg" ? t.TextTg : t.TextRu,
            isPopular = t.IsPopular
        });
        return Ok(ApiResponse<object>.Ok(dtos));
    }
}
