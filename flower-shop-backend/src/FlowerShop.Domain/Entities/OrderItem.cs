using System.Text.Json;

namespace FlowerShop.Domain.Entities;

public class OrderItem
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public Guid? ProductId { get; set; }
    public JsonDocument ProductSnapshot { get; set; } = null!;
    public string SizeLabel { get; set; } = string.Empty;
    public int Quantity { get; set; } = 1;
    public int UnitPrice { get; set; }
    public int? StemCount { get; set; }
    public string Packaging { get; set; } = "no_basket";

    public Order Order { get; set; } = null!;
    public Product? Product { get; set; }
}
