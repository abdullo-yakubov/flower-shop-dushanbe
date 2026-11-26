using FlowerShop.Domain.Enums;

namespace FlowerShop.Domain.Entities;

public class User
{
    public Guid Id { get; set; }
    public string Phone { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public UserRole Role { get; set; } = UserRole.Customer;
    public string? PasswordHash { get; set; }
    public bool IsBlocked { get; set; }
    public string? BlockReason { get; set; }
    public int TotalOrders { get; set; }
    public int TotalSpent { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    public LoyaltyAccount? LoyaltyAccount { get; set; }
    public ICollection<RefreshToken> RefreshTokens { get; set; } = [];
    public ICollection<Order> Orders { get; set; } = [];
    public ICollection<Address> Addresses { get; set; } = [];
    public ICollection<Review> Reviews { get; set; } = [];
}
