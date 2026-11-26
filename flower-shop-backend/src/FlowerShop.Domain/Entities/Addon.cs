namespace FlowerShop.Domain.Entities;

public class Addon
{
    public Guid Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public string NameRu { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string NameTg { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public int Price { get; set; }
    public bool HasInscription { get; set; }
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }
}
