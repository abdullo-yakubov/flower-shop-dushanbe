namespace FlowerShop.Domain.Entities;

public class OtpCode
{
    public Guid Id { get; set; }
    public string Phone { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public int Attempts { get; set; }
    public bool IsUsed { get; set; }
    public DateTimeOffset ExpiresAt { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
