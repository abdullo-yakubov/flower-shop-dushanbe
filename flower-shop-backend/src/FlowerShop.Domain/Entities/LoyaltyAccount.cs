using FlowerShop.Domain.Enums;

namespace FlowerShop.Domain.Entities;

public class LoyaltyAccount
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public int Points { get; set; }
    public int TotalEarned { get; set; }
    public LoyaltyTier Tier { get; set; } = LoyaltyTier.Bronze;
    public DateTimeOffset UpdatedAt { get; set; }

    public User User { get; set; } = null!;
    public ICollection<LoyaltyTransaction> Transactions { get; set; } = [];
}
