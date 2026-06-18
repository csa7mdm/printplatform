using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PrintPlatform.Loyalty.Models;

namespace PrintPlatform.Loyalty.Data.Configurations;

internal sealed class RedemptionConfiguration : IEntityTypeConfiguration<Redemption>
{
    public void Configure(EntityTypeBuilder<Redemption> b)
    {
        b.ToTable("loy_redemptions");

        b.HasKey(r => r.Id);
        b.Property(r => r.Id).UseIdentityColumn();

        b.Property(r => r.OrderReference).HasMaxLength(100);

        b.HasOne(r => r.Member)
         .WithMany()
         .HasForeignKey(r => r.MemberId)
         .OnDelete(DeleteBehavior.Cascade);

        b.HasOne(r => r.Reward)
         .WithMany(rw => rw.Redemptions)
         .HasForeignKey(r => r.RewardId)
         .OnDelete(DeleteBehavior.Restrict);
    }
}
