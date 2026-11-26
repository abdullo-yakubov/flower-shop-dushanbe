using FlowerShop.Domain.Constants;
using FlowerShop.Domain.Entities;
using FlowerShop.Domain.Enums;
using FlowerShop.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace FlowerShop.Infrastructure.Services;

/// <summary>
/// Loyalty ledger. Methods only stage changes on the DbContext — the caller owns
/// the transaction and calls SaveChanges, so an order and its points commit atomically.
/// </summary>
public class LoyaltyService(AppDbContext db)
{
    public async Task<int> EarnPointsAsync(Guid userId, int orderTotal, Order order, CancellationToken ct)
    {
        var earned = orderTotal / LoyaltyConstants.EarnDivisor;
        if (earned <= 0) return 0;

        var account = await GetOrCreateAsync(userId, ct);
        account.Points += earned;
        account.TotalEarned += earned;
        Touch(account);
        AddTransaction(account, order, LoyaltyTransactionType.Earn, earned, $"Начислено за заказ {order.OrderNumber}");
        return earned;
    }

    public async Task RedeemPointsAsync(Guid userId, int points, Order order, CancellationToken ct)
    {
        if (points <= 0) return;

        var account = await GetOrCreateAsync(userId, ct);
        if (points > account.Points)
            throw new AppException("VALIDATION_ERROR", "Недостаточно баллов");

        account.Points -= points;
        Touch(account);
        AddTransaction(account, order, LoyaltyTransactionType.Redeem, -points, $"Списано за заказ {order.OrderNumber}");
    }

    /// <summary>On cancellation: return redeemed points and take back points earned for the order.</summary>
    public async Task RevertOrderAsync(Guid userId, Order order, CancellationToken ct)
    {
        var txs = await db.LoyaltyTransactions.Where(t => t.OrderId == order.Id).ToListAsync(ct);
        var redeemed = -txs.Where(t => t.Type == LoyaltyTransactionType.Redeem && t.Amount < 0).Sum(t => t.Amount);
        var earned = txs.Where(t => t.Type == LoyaltyTransactionType.Earn && t.Amount > 0).Sum(t => t.Amount);
        if (redeemed == 0 && earned == 0) return;

        var account = await GetOrCreateAsync(userId, ct);

        if (redeemed > 0)
        {
            account.Points += redeemed;
            AddTransaction(account, order, LoyaltyTransactionType.Earn, redeemed, $"Возврат баллов: отмена заказа {order.OrderNumber}");
        }

        if (earned > 0)
        {
            var revoke = Math.Min(earned, account.Points);
            account.Points -= revoke;
            account.TotalEarned = Math.Max(0, account.TotalEarned - earned);
            if (revoke > 0)
                AddTransaction(account, order, LoyaltyTransactionType.Redeem, -revoke, $"Отмена начисления: заказ {order.OrderNumber}");
        }

        Touch(account);
    }

    private async Task<LoyaltyAccount> GetOrCreateAsync(Guid userId, CancellationToken ct)
    {
        var account = db.LoyaltyAccounts.Local.FirstOrDefault(a => a.UserId == userId)
            ?? await db.LoyaltyAccounts.FirstOrDefaultAsync(a => a.UserId == userId, ct);
        if (account is not null) return account;

        account = new LoyaltyAccount { Id = Guid.NewGuid(), UserId = userId, UpdatedAt = DateTimeOffset.UtcNow };
        db.LoyaltyAccounts.Add(account);
        return account;
    }

    private void AddTransaction(LoyaltyAccount account, Order order, LoyaltyTransactionType type, int amount, string description) =>
        db.LoyaltyTransactions.Add(new LoyaltyTransaction
        {
            UserId = account.UserId,
            OrderId = order.Id,
            Type = type,
            Amount = amount,
            BalanceAfter = account.Points,
            Description = description,
            CreatedAt = DateTimeOffset.UtcNow
        });

    private static void Touch(LoyaltyAccount account)
    {
        account.Tier = RecalcTier(account.TotalEarned);
        account.UpdatedAt = DateTimeOffset.UtcNow;
    }

    public static LoyaltyTier RecalcTier(int totalEarned) => totalEarned switch
    {
        >= LoyaltyConstants.GoldThreshold => LoyaltyTier.Gold,
        >= LoyaltyConstants.SilverThreshold => LoyaltyTier.Silver,
        _ => LoyaltyTier.Bronze
    };
}
