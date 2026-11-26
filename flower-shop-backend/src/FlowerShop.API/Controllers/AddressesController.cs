using FlowerShop.Application.DTOs.Common;
using FlowerShop.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FlowerShop.API.Controllers;

[ApiController]
[Route("api/v1/addresses")]
[Authorize]
public class AddressesController(AppDbContext db) : ControllerBase
{
    private Guid UserId => Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet]
    public async Task<IActionResult> GetAddresses(CancellationToken ct)
    {
        var list = await db.Addresses.Where(a => a.UserId == UserId).OrderByDescending(a => a.IsDefault).ToListAsync(ct);
        return Ok(ApiResponse<object>.Ok(list.Select(a => Map(a))));
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] AddressRequest req, CancellationToken ct)
    {
        var count = await db.Addresses.CountAsync(a => a.UserId == UserId, ct);
        if (count >= 10) return BadRequest(ApiResponse<object>.Fail("CONFLICT", "Максимум 10 адресов"));

        var addr = new Domain.Entities.Address
        {
            UserId = UserId, Label = req.Label, FullAddress = req.FullAddress,
            CreatedAt = DateTimeOffset.UtcNow, UpdatedAt = DateTimeOffset.UtcNow
        };
        db.Addresses.Add(addr);
        await db.SaveChangesAsync(ct);
        return Created(string.Empty, ApiResponse<object>.Ok(Map(addr)));
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] AddressRequest req, CancellationToken ct)
    {
        var addr = await db.Addresses.FirstOrDefaultAsync(a => a.Id == id && a.UserId == UserId, ct);
        if (addr is null) return NotFound();
        addr.Label = req.Label; addr.FullAddress = req.FullAddress; addr.UpdatedAt = DateTimeOffset.UtcNow;
        await db.SaveChangesAsync(ct);
        return Ok(ApiResponse<object>.Ok(Map(addr)));
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        var addr = await db.Addresses.FirstOrDefaultAsync(a => a.Id == id && a.UserId == UserId, ct);
        if (addr is null) return NotFound();
        db.Addresses.Remove(addr);
        await db.SaveChangesAsync(ct);
        return Ok(new { success = true });
    }

    [HttpPatch("{id}/default")]
    public async Task<IActionResult> SetDefault(Guid id, CancellationToken ct)
    {
        await db.Addresses.Where(a => a.UserId == UserId).ExecuteUpdateAsync(s => s.SetProperty(a => a.IsDefault, false), ct);
        var addr = await db.Addresses.FirstOrDefaultAsync(a => a.Id == id && a.UserId == UserId, ct);
        if (addr is null) return NotFound();
        addr.IsDefault = true;
        await db.SaveChangesAsync(ct);
        return Ok(new { success = true });
    }

    private static object Map(Domain.Entities.Address a) => new { id = a.Id, label = a.Label, fullAddress = a.FullAddress, isDefault = a.IsDefault, createdAt = a.CreatedAt };
}

public record AddressRequest(string Label, string FullAddress);
