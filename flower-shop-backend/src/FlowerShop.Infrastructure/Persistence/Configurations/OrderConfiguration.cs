using FlowerShop.Domain.Entities;
using FlowerShop.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FlowerShop.Infrastructure.Persistence.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("orders");
        builder.HasKey(o => o.Id);
        builder.Property(o => o.Id).HasDefaultValueSql("gen_random_uuid()");
        builder.Property(o => o.OrderNumber).HasMaxLength(20).IsRequired();
        builder.Property(o => o.Status).HasConversion<string>().HasMaxLength(30).HasDefaultValue(OrderStatus.Accepted);
        builder.Property(o => o.Address).IsRequired();
        builder.Property(o => o.DeliveryTime).HasMaxLength(20).IsRequired();
        builder.Property(o => o.PaymentMethod).HasConversion<string>().HasMaxLength(20);
        builder.Property(o => o.CustomerName).HasMaxLength(100).IsRequired();
        builder.Property(o => o.CustomerPhone).HasMaxLength(20).IsRequired();
        builder.Property(o => o.RecipientName).HasMaxLength(100);
        builder.Property(o => o.RecipientPhone).HasMaxLength(20);
        builder.Property(o => o.PromoCode).HasMaxLength(30);
        builder.Property(o => o.CancelReason).HasMaxLength(255);
        builder.Property(o => o.CreatedAt).HasDefaultValueSql("now()");
        builder.Property(o => o.UpdatedAt).HasDefaultValueSql("now()");

        builder.HasIndex(o => o.UserId);
        builder.HasIndex(o => o.Status);
        builder.HasIndex(o => o.CreatedAt).IsDescending();
        builder.HasIndex(o => o.OrderNumber).IsUnique();

        builder.HasOne(o => o.User).WithMany(u => u.Orders).HasForeignKey(o => o.UserId).OnDelete(DeleteBehavior.SetNull);
        builder.HasOne(o => o.DeliveryZone).WithMany().HasForeignKey(o => o.DeliveryZoneId).OnDelete(DeleteBehavior.SetNull);
        builder.HasOne(o => o.Postcard).WithMany().HasForeignKey(o => o.PostcardId).OnDelete(DeleteBehavior.SetNull);
        builder.HasMany(o => o.Items).WithOne(i => i.Order).HasForeignKey(i => i.OrderId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(o => o.Addons).WithOne(a => a.Order).HasForeignKey(a => a.OrderId).OnDelete(DeleteBehavior.Cascade);
        builder.HasMany(o => o.StatusHistory).WithOne(h => h.Order).HasForeignKey(h => h.OrderId).OnDelete(DeleteBehavior.Cascade);
    }
}
