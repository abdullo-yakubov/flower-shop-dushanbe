using FlowerShop.Domain.Entities;
using FlowerShop.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FlowerShop.Infrastructure.Persistence.Configurations;

public class ProductOccasionConfiguration : IEntityTypeConfiguration<ProductOccasion>
{
    public void Configure(EntityTypeBuilder<ProductOccasion> builder)
    {
        builder.ToTable("product_occasions");
        builder.HasKey(po => new { po.ProductId, po.Occasion });
        builder.Property(po => po.Occasion).HasConversion<string>().HasMaxLength(20);
    }
}
