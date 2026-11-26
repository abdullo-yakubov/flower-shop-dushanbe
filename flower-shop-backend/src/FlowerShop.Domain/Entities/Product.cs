using FlowerShop.Domain.Enums;

namespace FlowerShop.Domain.Entities;

public class Product
{
    public Guid Id { get; set; }
    public string Slug { get; set; } = string.Empty;
    public string NameRu { get; set; } = string.Empty;
    public string NameEn { get; set; } = string.Empty;
    public string NameTg { get; set; } = string.Empty;
    public string CompositionRu { get; set; } = string.Empty;
    public string CompositionEn { get; set; } = string.Empty;
    public string CompositionTg { get; set; } = string.Empty;
    public int Price { get; set; }
    public int? OldPrice { get; set; }
    public int? Discount { get; set; }
    public decimal Rating { get; set; }
    public int ReviewCount { get; set; }
    public bool IsNew { get; set; }
    public bool IsHit { get; set; }
    public bool InStock { get; set; } = true;
    public bool DeliverToday { get; set; }
    public ProductKind Kind { get; set; }
    public Freshness Freshness { get; set; }
    public int? StemPrice { get; set; }
    public int[] StemOptions { get; set; } = [];
    public int SortOrder { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTimeOffset CreatedAt { get; set; }
    public DateTimeOffset UpdatedAt { get; set; }

    public ICollection<ProductImage> Images { get; set; } = [];
    public ICollection<ProductSize> Sizes { get; set; } = [];
    public ICollection<ProductOccasion> Occasions { get; set; } = [];
    public ICollection<ProductFlowerType> FlowerTypes { get; set; } = [];
    public ICollection<ProductColor> Colors { get; set; } = [];
    public ICollection<ProductPackaging> PackagingOptions { get; set; } = [];
    public ICollection<Review> Reviews { get; set; } = [];
}
