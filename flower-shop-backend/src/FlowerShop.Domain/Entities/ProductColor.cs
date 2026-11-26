namespace FlowerShop.Domain.Entities;

public class ProductColor
{
    public Guid ProductId { get; set; }
    public string Color { get; set; } = string.Empty;

    public Product Product { get; set; } = null!;
}
