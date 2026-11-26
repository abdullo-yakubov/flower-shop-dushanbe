namespace FlowerShop.Domain.Entities;

public class PushSubscription
{
    public Guid Id { get; set; }
    public Guid? UserId { get; set; }
    public string Endpoint { get; set; } = string.Empty;
    public string P256dh { get; set; } = string.Empty;
    public string AuthKey { get; set; } = string.Empty;
    public DateTimeOffset CreatedAt { get; set; }

    public User? User { get; set; }
}
