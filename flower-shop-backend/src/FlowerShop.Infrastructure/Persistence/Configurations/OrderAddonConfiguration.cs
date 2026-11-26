using FlowerShop.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FlowerShop.Infrastructure.Persistence.Configurations;

public class OrderAddonConfiguration : IEntityTypeConfiguration<OrderAddon>
{
    public void Configure(EntityTypeBuilder<OrderAddon> builder)
    {
        builder.ToTable("order_addons");
        builder.HasKey(oa => oa.Id);
        builder.Property(oa => oa.Id).HasDefaultValueSql("gen_random_uuid()");
        builder.Property(oa => oa.AddonSnapshot).HasColumnType("jsonb");
        builder.Property(oa => oa.Inscription).HasMaxLength(40);
        builder.HasIndex(oa => oa.OrderId);
        builder.HasOne(oa => oa.Addon).WithMany().HasForeignKey(oa => oa.AddonId).OnDelete(DeleteBehavior.SetNull);
    }
}
