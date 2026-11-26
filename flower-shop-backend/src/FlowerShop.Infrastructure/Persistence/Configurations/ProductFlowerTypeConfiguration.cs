using FlowerShop.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FlowerShop.Infrastructure.Persistence.Configurations;

public class ProductFlowerTypeConfiguration : IEntityTypeConfiguration<ProductFlowerType>
{
    public void Configure(EntityTypeBuilder<ProductFlowerType> builder)
    {
        builder.ToTable("product_flower_types");
        builder.HasKey(pf => new { pf.ProductId, pf.FlowerType });
        builder.Property(pf => pf.FlowerType).HasConversion<string>().HasMaxLength(20);
    }
}
