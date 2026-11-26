using FlowerShop.Domain.Constants;
using FlowerShop.Domain.Entities;
using FlowerShop.Domain.Enums;
using FlowerShop.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FlowerShop.Infrastructure.Services;

public class LoyaltyService(AppDbContext db)
{
    public async Task<int> EarnPointsAsync(Guid userId, int orderTotal, Guid orderId, CancellationToken ct)
    {
        var account = await GetOrCreateAsync(userId, ct);
        var earned = orderTotal / LoyaltyConstants.EarnDivisor;
        if (earned <= 0) return 0;

        account.Points += earned;
        account.TotalEarned += earned;
        account.Tier = RecalcTier(account.TotalEarned);
        account.UpdatedAt = DateTimeOffset.UtcNow;

        db.LoyaltyTransactions.Add(new LoyaltyTransaction
        {
            UserId = userId,
            OrderId = orderId,
            Type = LoyaltyTransactionType.Earn,
            Amount = earned,
            BalanceAfter = account.Points,
            Description = $"За заказ #{orderId}",
            CreatedAt = DateTimeOffset.UtcNow
        });

        return earned;
    }

    public async Task RedeemPointsAsync(Guid userId, int points, Guid orderId, CancellationToken ct)
    {
        var account = await GetOrCreateAsync(userId, ct);
        if (points < LoyaltyConstants.MinRedeem)
            throw new AppException("VALIDATION_ERROR", $"Минимум для списания: {LoyaltyConstants.MinRedeem} баллов");
        if (points > account.Points)
            throw new AppException("VALIDATION_ERROR", "Недостаточно баллов");

        account.Points -= points;
        account.UpdatedAt = DateTimeOffset.UtcNow;

        db.LoyaltyTransactions.Add(new LoyaltyTransaction
        {
            UserId = userId,
            OrderId = orderId,
            Type = LoyaltyTransactionType.Redeem,
            Amount = -points,
            BalanceAfter = account.Points,
            Description = $"Списание баллов за заказ #{orderId}",
            CreatedAt = DateTimeOffset.UtcNow
        });
    }

    public async Task AdjustPointsAsync(Guid userId, int amount, string description, CancellationToken ct)
    {
        var account = await GetOrCreateAsync(userId, ct);
        account.Points += amount;
        if (amount > 0) account.TotalEarned += amount;
        account.Tier = RecalcTier(account.TotalEarned);
        account.UpdatedAt = DateTimeOffset.UtcNow;

        db.LoyaltyTransactions.Add(new LoyaltyTransaction
        {
            UserId = userId,
            Type = LoyaltyTransactionType.AdminAdjust,
            Amount = amount,
            BalanceAfter = account.Points,
            Description = description,
            CreatedAt = DateTimeOffset.UtcNow
        });

        await db.SaveChangesAsync(ct);
    }

    private async Task<LoyaltyAccount> GetOrCreateAsync(Guid userId, CancellationToken ct)
    {
        var account = await db.LoyaltyAccounts.FirstOrDefaultAsync(a => a.UserId == userId, ct);
        if (account is null)
        {
            account = new LoyaltyAccount { Id = Guid.NewGuid(), UserId = userId, UpdatedAt = DateTimeOffset.UtcNow };
            db.LoyaltyAccounts.Add(account);
            await db.SaveChangesAsync(ct);
        }
        return account;
    }

    private static LoyaltyTier RecalcTier(int totalEarned) => totalEarned switch
    {
        >= LoyaltyConstants.GoldThreshold => LoyaltyTier.Gold,
        >= LoyaltyConstants.SilverThreshold => LoyaltyTier.Silver,
        _ => LoyaltyTier.Bronze
    };
}
