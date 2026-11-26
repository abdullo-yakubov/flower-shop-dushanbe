using FlowerShop.Domain.Enums;

namespace FlowerShop.Domain.Entities;

public class ProductSize
{
    public Guid Id { get; set; }
    public Guid ProductId { get; set; }
    public SizeLabel Label { get; set; }
    public int Price { get; set; }
    public int? OldPrice { get; set; }
    public string? ImageUrl { get; set; }
    public int SortOrder { get; set; }

    public Product Product { get; set; } = null!;
}
