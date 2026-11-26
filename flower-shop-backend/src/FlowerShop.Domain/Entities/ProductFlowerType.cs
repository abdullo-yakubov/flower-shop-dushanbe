using FlowerShop.Domain.Enums;

namespace FlowerShop.Domain.Entities;

public class ProductFlowerType
{
    public Guid ProductId { get; set; }
    public FlowerType FlowerType { get; set; }

    public Product Product { get; set; } = null!;
}
