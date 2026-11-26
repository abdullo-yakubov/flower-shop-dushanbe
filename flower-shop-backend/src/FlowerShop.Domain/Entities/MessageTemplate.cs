namespace FlowerShop.Domain.Entities;

public class MessageTemplate
{
    public Guid Id { get; set; }
    public string Category { get; set; } = string.Empty;
    public string TextRu { get; set; } = string.Empty;
    public string TextEn { get; set; } = string.Empty;
    public string TextTg { get; set; } = string.Empty;
    public bool IsPopular { get; set; }
    public bool IsActive { get; set; } = true;
    public int SortOrder { get; set; }
    public DateTimeOffset CreatedAt { get; set; }
}
