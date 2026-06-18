using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PrintPlatform.Loyalty.Models;

namespace PrintPlatform.Loyalty.Data.Configurations;

internal sealed class LoyaltyTierConfiguration : IEntityTypeConfiguration<LoyaltyTier>
{
    public void Configure(EntityTypeBuilder<LoyaltyTier> b)
    {
        b.ToTable("loy_tiers");

        b.HasKey(t => t.Id);
        b.Property(t => t.Id).UseIdentityColumn();

        b.Property(t => t.Name).IsRequired().HasMaxLength(100);
        b.Property(t => t.BenefitsJson).IsRequired().HasColumnType("jsonb");
    }
}
