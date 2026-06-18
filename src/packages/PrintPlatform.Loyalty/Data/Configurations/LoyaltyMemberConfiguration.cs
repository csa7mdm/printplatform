using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PrintPlatform.Loyalty.Models;

namespace PrintPlatform.Loyalty.Data.Configurations;

internal sealed class LoyaltyMemberConfiguration : IEntityTypeConfiguration<LoyaltyMember>
{
    public void Configure(EntityTypeBuilder<LoyaltyMember> b)
    {
        b.ToTable("loy_members");

        b.HasKey(m => m.Id);
        b.Property(m => m.Id).UseIdentityColumn();

        b.Property(m => m.ExternalUserId).IsRequired().HasMaxLength(200);
        b.Property(m => m.LifetimePoints).IsRequired();
        b.Property(m => m.ActivePoints).IsRequired();
        b.Property(m => m.CompletedOrderCount).IsRequired();
        b.Property(m => m.ReferralCode).IsRequired().HasMaxLength(32);
        b.Property(m => m.CreatedAt).IsRequired();
        b.Property(m => m.UpdatedAt).IsRequired();

        b.HasOne(m => m.CurrentTier)
         .WithMany(t => t.Members)
         .HasForeignKey(m => m.CurrentTierId)
         .OnDelete(DeleteBehavior.SetNull);

        b.HasIndex(m => m.ExternalUserId).IsUnique();
        b.HasIndex(m => m.ReferralCode).IsUnique();
    }
}
