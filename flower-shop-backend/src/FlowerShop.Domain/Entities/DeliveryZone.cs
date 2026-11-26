namespace FlowerShop.Domain.Entities;

public class DeliveryZone
{
    public Guid Id { get; set; }
    public string NameRu { get; set; } = string.Empty;
    public int Fee { get; set; }
    public int MinFreeDelivery { get; set; } = 1500;
    public int MinHours { get; set; } = 1;
    public int MaxHours { get; set; } = 3;
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
