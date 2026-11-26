namespace FlowerShop.Application.DTOs.Loyalty;

public record LoyaltyDto(
    int Points,
    int TotalEarned,
    string Tier,
    string TierLabel,
    int NextTierPoints,
    int PointsToNextTier,
    IEnumerable<LoyaltyTransactionDto> History
);

public record LoyaltyTransactionDto(
    Guid Id,
    string Type,
    int Amount,
    int BalanceAfter,
    string Description,
    DateTimeOffset CreatedAt
);
