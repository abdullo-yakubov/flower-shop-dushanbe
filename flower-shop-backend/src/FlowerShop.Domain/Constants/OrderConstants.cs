namespace FlowerShop.Domain.Constants;

public static class OrderConstants
{
    /// <summary>Canonical slot strings — identical to the frontend (en dash, U+2013).</summary>
    public static readonly string[] TimeSlots =
    [
        "09:00–12:00",
        "12:00–15:00",
        "15:00–18:00",
        "18:00–21:00"
    ];

    public const string NumberPrefix = "#";

    /// <summary>Basket packaging surcharge (materials + assembly), сомони. Mirrors flower-shop packagingConfig.ts.</summary>
    public const int BasketSurcharge = 80;

    /// <summary>Upper bound for quantity of a single cart line / addon. Mirrors MAX_CART_QUANTITY.</summary>
    public const int MaxLineQuantity = 20;

    /// <summary>Fallbacks used when no delivery zone is chosen and the setting is absent.</summary>
    public const int DefaultDeliveryFee = 40;
    public const int DefaultFreeDeliveryThreshold = 1500;

    /// <summary>
    /// Accepts both "09:00-12:00" (hyphen) and "09:00–12:00" (en dash) and returns the canonical slot,
    /// or null when the slot is unknown.
    /// </summary>
    public static string? NormalizeTimeSlot(string? slot)
    {
        if (string.IsNullOrWhiteSpace(slot)) return null;
        var normalized = slot.Trim().Replace('-', '–').Replace('—', '–').Replace(" ", "");
        return TimeSlots.FirstOrDefault(s => s == normalized);
    }
}
