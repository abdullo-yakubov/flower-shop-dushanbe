using FlowerShop.Domain.Entities;
using FlowerShop.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FlowerShop.Infrastructure.Persistence.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("products");
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Id).HasDefaultValueSql("gen_random_uuid()");
        builder.Property(p => p.Slug).HasMaxLength(100).IsRequired();
        builder.Property(p => p.NameRu).HasMaxLength(200).IsRequired();
        builder.Property(p => p.NameEn).HasMaxLength(200).HasDefaultValue(string.Empty);
        builder.Property(p => p.NameTg).HasMaxLength(200).HasDefaultValue(string.Empty);
        builder.Property(p => p.Rating).HasColumnType("numeric(3,2)");
        builder.Property(p => p.Kind).HasConversion<string>().HasMaxLength(20);
        builder.Property(p => p.Freshness).HasConversion<string>().HasMaxLength(20);
        builder.Property(p => p.StemOptions).HasColumnType("integer[]");
        builder.Property(p => p.CreatedAt).HasDefaultValueSql("now()");
        builder.Property(p => p.UpdatedAt).HasDefaultValueSql("now()");

        builder.HasIndex(p => p.Slug).IsUnique();
        builder.HasIndex(p => p.IsActive);
        builder.HasIndex(p => p.Kind);
        builder.HasIndex(p => p.InStock);

        builder.HasMany(p => p.Images).WithOne(i => i.Product).HasForeignKey(i => i.ProductId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(p => p.Sizes).WithOne(s => s.Product).HasForeignKey(s => s.ProductId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(p => p.Occasions).WithOne(o => o.Product).HasForeignKey(o => o.ProductId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(p => p.FlowerTypes).WithOne(f => f.Product).HasForeignKey(f => f.ProductId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(p => p.Colors).WithOne(c => c.Product).HasForeignKey(c => c.ProductId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(p => p.PackagingOptions).WithOne(pk => pk.Product).HasForeignKey(pk => pk.ProductId).OnDelete(DeleteBehavior.Cascade);
    }
}
