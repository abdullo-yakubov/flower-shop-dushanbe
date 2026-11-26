using FlowerShop.Domain.Enums;

namespace FlowerShop.Domain.Entities;

public class Order
{
    public Guid Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;
    public Guid? UserId { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.Accepted;

    public int Subtotal { get; set; }
    public int DeliveryFee { get; set; }
    public int PromoDiscount { get; set; }
    public int PointsDiscount { get; set; }
    public int Total { get; set; }

    public string Address { get; set; } = string.Empty;
    public Guid? DeliveryZoneId { get; set; }
    public DateOnly DeliveryDate { get; set; }
    public string DeliveryTime { get; set; } = string.Empty;

    public PaymentMethod PaymentMethod { get; set; }
    public bool IsPaid { get; set; }

    public string CustomerName { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
    public bool IsGift { get; set; }
    public string? RecipientName { get; set; }
    public string? RecipientPhone { get; set; }
    public bool IsAnonymous { get; set; }

    public Guid? PostcardId { get; set; }
    public string? CardText { get; set; }

    public string? PromoCode { get; set; }

    public bool IsCancellable { get; set; } = true;
    public string? CancelReason { get; set; }
    public string? AdminNote { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    public User? User { get; set; }
    public DeliveryZone? DeliveryZone { get; set; }
    public Postcard? Postcard { get; set; }
    public ICollection<OrderItem> Items { get; set; } = [];
    public ICollection<OrderAddon> Addons { get; set; } = [];
    public ICollection<OrderStatusHistory> StatusHistory { get; set; } = [];
}
