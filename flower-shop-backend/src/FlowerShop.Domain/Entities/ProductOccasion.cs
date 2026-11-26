using FlowerShop.Domain.Enums;

namespace FlowerShop.Domain.Entities;

public class ProductOccasion
{
    public Guid ProductId { get; set; }
    public Occasion Occasion { get; set; }

    public Product Product { get; set; } = null!;
}
