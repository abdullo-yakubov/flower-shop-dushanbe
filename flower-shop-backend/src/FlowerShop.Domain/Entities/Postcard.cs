namespace FlowerShop.Domain.Entities;

public class Postcard
{
    public Guid Id { get; set; }
    public string Occasion { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public string Emoji { get; set; } = "🌸";
    public string NameRu { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string NameTg { get; set; } = string.Empty;
    public bool IsPopular { get; set; }
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
