using FlowerShop.Application.DTOs.Common;
using FlowerShop.Infrastructure.Persistence;
using FlowerShop.Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FlowerShop.API.Controllers;

[ApiController]
[Route("api/v1/account")]
[Authorize]
public class AccountController(AppDbContext db, IFileStorageService fileStorage) : ControllerBase
{
    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet("profile")]
    public async Task<IActionResult> GetProfile(CancellationToken ct)
    {
        var user = await db.Users.FindAsync([UserId], ct);
        if (user is null) return NotFound();

        return Ok(ApiResponse<object>.Ok(new
        {
            id = user.Id,
            name = user.Name,
            phone = user.Phone,
            avatarUrl = user.AvatarUrl,
            totalOrders = user.TotalOrders,
            totalSpent = user.TotalSpent,
            createdAt = user.CreatedAt
        }));
    }

    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest req, CancellationToken ct)
    {
        var user = await db.Users.FindAsync([UserId], ct);
        if (user is null) return NotFound();

        user.Name = req.Name;
        user.UpdatedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(ct);

        return Ok(new { success = true });
    }

    [HttpPost("avatar")]
    public async Task<IActionResult> UploadAvatar(IFormFile avatar, CancellationToken ct)
    {
        if (avatar is null || avatar.Length == 0)
            return BadRequest(ApiResponse<object>.Fail("VALIDATION_ERROR", "Файл не выбран"));
        if (avatar.Length > 5 * 1024 * 1024)
            return BadRequest(ApiResponse<object>.Fail("VALIDATION_ERROR", "Файл слишком большой (макс. 5МБ)"));

        var user = await db.Users.FindAsync([UserId], ct);
        if (user is null) return NotFound();

        var url = await fileStorage.SaveAsync(avatar, "avatars", 200, 200, ct);
        user.AvatarUrl = url;
        user.UpdatedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(ct);

        return Ok(ApiResponse<object>.Ok(new { avatarUrl = url }));
    }
}

public record UpdateProfileRequest(string Name);
