namespace FlowerShop.Domain.Entities;

public class HolidayPricing
{
    public Guid Id { get; set; }
    public string NameRu { get; set; } = string.Empty;
    public int Month { get; set; }
    public int Day { get; set; }
    public int MarkupPercent { get; set; } = 30;
    public string AppliesTo { get; set; } = "live";
    public bool IsActive { get; set; } = true;
}
