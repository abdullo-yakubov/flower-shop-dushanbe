using FlowerShop.Domain.Enums;

namespace FlowerShop.Domain.Entities;

public class ProductPackaging
{
    public Guid ProductId { get; set; }
    public Packaging Packaging { get; set; }

    public Product Product { get; set; } = null!;
}
