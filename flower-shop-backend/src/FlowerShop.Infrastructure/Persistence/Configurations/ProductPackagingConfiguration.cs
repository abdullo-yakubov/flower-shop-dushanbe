using FlowerShop.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FlowerShop.Infrastructure.Persistence.Configurations;

public class ProductPackagingConfiguration : IEntityTypeConfiguration<ProductPackaging>
{
    public void Configure(EntityTypeBuilder<ProductPackaging> builder)
    {
        builder.ToTable("product_packaging");
        builder.HasKey(pp => new { pp.ProductId, pp.Packaging });
        builder.Property(pp => pp.Packaging).HasConversion<string>().HasMaxLength(20);
    }
}
