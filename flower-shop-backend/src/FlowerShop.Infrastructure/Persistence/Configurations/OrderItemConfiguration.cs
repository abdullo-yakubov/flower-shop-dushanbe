using FlowerShop.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FlowerShop.Infrastructure.Persistence.Configurations;

public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> builder)
    {
        builder.ToTable("order_items");
        builder.HasKey(oi => oi.Id);
        builder.Property(oi => oi.Id).HasDefaultValueSql("gen_random_uuid()");
        builder.Property(oi => oi.ProductSnapshot).HasColumnType("jsonb");
        builder.Property(oi => oi.SizeLabel).HasMaxLength(10).IsRequired();
        builder.Property(oi => oi.Packaging).HasMaxLength(20).HasDefaultValue("no_basket");
        builder.HasIndex(oi => oi.OrderId);
        builder.HasOne(oi => oi.Product).WithMany().HasForeignKey(oi => oi.ProductId).OnDelete(DeleteBehavior.SetNull);
    }
}
