namespace FlowerShop.Application.DTOs.Products;

public record ProductCardDto(
    Guid Id,
    string Slug,
    string Name,
    string Composition,
    IEnumerable<string> Images,
    int Price,
    int? OldPrice,
    int? Discount,
    decimal Rating,
    int ReviewCount,
    IEnumerable<string> Occasions,
    IEnumerable<string> FlowerTypes,
    IEnumerable<string> Colors,
    bool IsNew,
    bool IsHit,
    bool InStock,
    bool DeliverToday,
    string Kind,
    string Freshness,
    IEnumerable<string> PackagingOptions,
    IEnumerable<ProductSizeDto> Sizes,
    int? StemPrice,
    IEnumerable<int> StemOptions
);

public record ProductSizeDto(Guid Id, string Label, int Price, int? OldPrice);

public record ProductsQuery(
    string? Q = null,
    string? Occasion = null,
    string? FlowerTypes = null,
    string? Colors = null,
    int? PriceMin = null,
    int? PriceMax = null,
    bool? DeliverToday = null,
    string? Freshness = null,
    string? Kind = null,
    string? Packaging = null,
    string Sort = "popular",
    int Page = 1,
    int PageSize = 20,
    string Lang = "ru"
);

public record ReviewDto(
    Guid Id,
    string AuthorName,
    string? AuthorAvatar,
    int Rating,
    string Text,
    IEnumerable<string> Photos,
    int HelpfulCount,
    string? AdminReply,
    string Date
);

public record CreateReviewRequest(int Rating, string Text, IEnumerable<string>? Photos = null);
