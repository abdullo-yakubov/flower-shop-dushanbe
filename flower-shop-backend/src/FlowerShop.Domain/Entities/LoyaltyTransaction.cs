using FlowerShop.Domain.Enums;

namespace FlowerShop.Domain.Entities;

public class LoyaltyTransaction
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public Guid? OrderId { get; set; }
    public LoyaltyTransactionType Type { get; set; }
    public int Amount { get; set; }
    public int BalanceAfter { get; set; }
    public string Description { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }

    public User User { get; set; } = null!;
    public Order? Order { get; set; }
}
