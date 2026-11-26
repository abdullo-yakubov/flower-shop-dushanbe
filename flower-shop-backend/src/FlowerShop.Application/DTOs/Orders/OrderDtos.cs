namespace FlowerShop.Application.DTOs.Orders;

public record CreateOrderRequest(
    IEnumerable<CreateOrderItem> Items,
    IEnumerable<CreateOrderAddon>? Addons,
    Guid? PostcardId,
    string? CardText,
    string Address,
    Guid? DeliveryZoneId,
    string DeliveryDate,
    string DeliveryTime,
    string PaymentMethod,
    string CustomerName,
    string CustomerPhone,
    bool IsGift = false,
    string? RecipientName = null,
    string? RecipientPhone = null,
    bool IsAnonymous = false,
    string? PromoCode = null,
    int RedeemPoints = 0
);

public record CreateOrderItem(
    Guid ProductId,
    Guid SizeId,
    int Quantity = 1,
    string Packaging = "no_basket",
    int? StemCount = null
);

public record CreateOrderAddon(
    Guid AddonId,
    int Quantity = 1,
    string? Inscription = null
);

public record CreateOrderResponse(
    Guid OrderId,
    string OrderNumber,
    int Total,
    int EarnedPoints,
    string Status
);

public record OrderListItemDto(
    Guid Id,
    string OrderNumber,
    string Status,
    int Total,
    string DeliveryDate,
    string DeliveryTime,
    int ItemCount,
    string? FirstItemImage,
    bool IsCancellable,
    DateTimeOffset CreatedAt
);

public record CancelOrderRequest(string? Reason);

public record PromoValidateRequest(string Code, int OrderTotal);
public record PromoValidateResponse(string Code, int DiscountPercent, int DiscountFixed, int DiscountAmount, string Label);
