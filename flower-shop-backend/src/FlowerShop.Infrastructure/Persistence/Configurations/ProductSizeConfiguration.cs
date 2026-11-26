using FlowerShop.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FlowerShop.Infrastructure.Persistence.Configurations;

public class ProductSizeConfiguration : IEntityTypeConfiguration<ProductSize>
{
    public void Configure(EntityTypeBuilder<ProductSize> builder)
    {
        builder.ToTable("product_sizes");
        builder.HasKey(ps => ps.Id);
        builder.Property(ps => ps.Id).HasDefaultValueSql("gen_random_uuid()");
        builder.Property(ps => ps.Label).HasConversion<string>().HasMaxLength(1);
        builder.Property(ps => ps.ImageUrl).HasMaxLength(500);
        builder.HasIndex(ps => ps.ProductId);
    }
}
