using FlowerShop.Application.DTOs.Common;
using FlowerShop.Application.DTOs.Loyalty;
using FlowerShop.Domain.Constants;
using FlowerShop.Domain.Enums;
using FlowerShop.Infrastructure.Persistence;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FlowerShop.API.Controllers;

[ApiController]
[Route("api/v1/loyalty")]
[Authorize]
public class LoyaltyController(AppDbContext db) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetLoyalty(CancellationToken ct)
    {
        var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var account = await db.LoyaltyAccounts
            .Include(a => a.Transactions)
            .FirstOrDefaultAsync(a => a.UserId == userId, ct);

        var (nextTier, nextThreshold) = account?.Tier switch
        {
            LoyaltyTier.Bronze => ("silver", LoyaltyConstants.SilverThreshold),
            LoyaltyTier.Silver => ("gold", LoyaltyConstants.GoldThreshold),
            _ => ("gold", LoyaltyConstants.GoldThreshold)
        };

        var points = account?.Points ?? 0;
        var totalEarned = account?.TotalEarned ?? 0;
        var tier = account?.Tier ?? LoyaltyTier.Bronze;
        var tierLabel = tier switch { LoyaltyTier.Silver => "Серебро", LoyaltyTier.Gold => "Золото", _ => "Бронза" };

        var history = account?.Transactions
            .OrderByDescending(t => t.CreatedAt)
            .Take(20)
            .Select(t => new LoyaltyTransactionDto(t.Id, t.Type.ToString().ToLower(), t.Amount, t.BalanceAfter, t.Description, t.CreatedAt))
            ?? [];

        var dto = new LoyaltyDto(points, totalEarned, tier.ToString().ToLower(), tierLabel,
            nextThreshold, Math.Max(0, nextThreshold - totalEarned), history);

        return Ok(ApiResponse<LoyaltyDto>.Ok(dto));
    }
}
