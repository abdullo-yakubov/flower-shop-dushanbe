namespace FlowerShop.Domain.Entities;

public class PromoCode
{
    public Guid Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string LabelRu { get; set; } = string.Empty;
    public int DiscountPercent { get; set; }
    public int DiscountFixed { get; set; }
    public int MinOrder { get; set; }
    public int? MaxUses { get; set; }
    public int UsedCount { get; set; }
    public DateTimeOffset ValidFrom { get; set; }
    public DateTimeOffset? ValidTo { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
