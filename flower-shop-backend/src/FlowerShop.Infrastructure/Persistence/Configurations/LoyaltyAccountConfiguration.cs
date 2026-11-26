using FlowerShop.Domain.Entities;
using FlowerShop.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace FlowerShop.Infrastructure.Persistence.Configurations;

public class LoyaltyAccountConfiguration : IEntityTypeConfiguration<LoyaltyAccount>
{
    public void Configure(EntityTypeBuilder<LoyaltyAccount> builder)
    {
        builder.ToTable("loyalty_accounts");
        builder.HasKey(la => la.Id);
        builder.Property(la => la.Id).HasDefaultValueSql("gen_random_uuid()");
        builder.Property(la => la.Tier).HasConversion<string>().HasMaxLength(20).HasDefaultValue(LoyaltyTier.Bronze);
        builder.Property(la => la.UpdatedAt).HasDefaultValueSql("now()");
        builder.HasIndex(la => la.UserId).IsUnique();
    }
}
