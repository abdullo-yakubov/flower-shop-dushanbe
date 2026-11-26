using System.Text.Json;

namespace FlowerShop.Domain.Entities;

public class OrderAddon
{
    public Guid Id { get; set; }
    public Guid OrderId { get; set; }
    public Guid? AddonId { get; set; }
    public JsonDocument AddonSnapshot { get; set; } = null!;
    public int Quantity { get; set; } = 1;
    public int UnitPrice { get; set; }
    public string? Inscription { get; set; }

    public Order Order { get; set; } = null!;
    public Addon? Addon { get; set; }
}
