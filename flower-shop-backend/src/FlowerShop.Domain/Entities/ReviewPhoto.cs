namespace FlowerShop.Domain.Entities;

public class ReviewPhoto
{
    public Guid Id { get; set; }
    public Guid ReviewId { get; set; }
    public string Url { get; set; } = string.Empty;
    public int SortOrder { get; set; }

    public Review Review { get; set; } = null!;
}
