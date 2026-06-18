using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PrintPlatform.Loyalty.Models;

namespace PrintPlatform.Loyalty.Data.Configurations;

internal sealed class ReferralConfiguration : IEntityTypeConfiguration<Referral>
{
    public void Configure(EntityTypeBuilder<Referral> b)
    {
        b.ToTable("loy_referrals");

        b.HasKey(r => r.Id);
        b.Property(r => r.Id).UseIdentityColumn();

        b.Property(r => r.ReferrerId).IsRequired().HasMaxLength(200);
        b.Property(r => r.ReferredUserId).IsRequired().HasMaxLength(200);
        b.Property(r => r.Code).IsRequired().HasMaxLength(32);
        b.Property(r => r.Status).HasConversion<string>().HasMaxLength(20);

        b.HasIndex(r => r.Code);
        b.HasIndex(r => r.ReferrerId);
        b.HasIndex(r => r.ReferredUserId);
    }
}
