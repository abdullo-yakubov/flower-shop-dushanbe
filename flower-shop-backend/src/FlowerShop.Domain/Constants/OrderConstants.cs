namespace FlowerShop.Domain.Constants;

public static class OrderConstants
{
    public static readonly string[] TimeSlots =
    [
        "09:00-12:00",
        "12:00-15:00",
        "15:00-18:00",
        "18:00-21:00"
    ];

    public const int MaxCancelMinutes = 0; // only 'accepted' status can be cancelled
    public const string NumberPrefix = "#";
}
