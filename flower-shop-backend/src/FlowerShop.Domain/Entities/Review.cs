namespace FlowerShop.Domain.Entities;

public class Review
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public Guid? UserId { get; set; }
    public Guid? OrderId { get; set; }
    public string AuthorName { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string Text { get; set; } = string.Empty;
    public bool IsApproved { get; set; }
    public bool IsHidden { get; set; }
    public int HelpfulCount { get; set; }
    public string? AdminReply { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    public Product Product { get; set; } = null!;
    public User? User { get; set; }
    public Order? Order { get; set; }
    public ICollection<ReviewPhoto> Photos { get; set; } = [];
}
