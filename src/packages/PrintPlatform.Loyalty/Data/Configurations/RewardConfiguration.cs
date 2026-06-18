using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PrintPlatform.Loyalty.Models;

namespace PrintPlatform.Loyalty.Data.Configurations;

internal sealed class RewardConfiguration : IEntityTypeConfiguration<Reward>
{
    public void Configure(EntityTypeBuilder<Reward> b)
    {
        b.ToTable("loy_rewards");

        b.HasKey(r => r.Id);
        b.Property(r => r.Id).UseIdentityColumn();

        b.Property(r => r.Name).IsRequired().HasMaxLength(200);
        b.Property(r => r.Description).IsRequired().HasMaxLength(1000);
        b.Property(r => r.Type).HasConversion<string>().HasMaxLength(20);
    }
}
