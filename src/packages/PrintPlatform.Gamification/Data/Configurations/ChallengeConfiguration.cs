using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PrintPlatform.Gamification.Models;

namespace PrintPlatform.Gamification.Data.Configurations;

internal sealed class ChallengeConfiguration : IEntityTypeConfiguration<Challenge>
{
    public void Configure(EntityTypeBuilder<Challenge> b)
    {
        b.ToTable("gam_challenges");

        b.HasKey(c => c.Id);
        b.Property(c => c.Id).UseIdentityColumn();

        b.Property(c => c.Name).IsRequired().HasMaxLength(150);
        b.Property(c => c.CriteriaJson).HasColumnType("jsonb").IsRequired();
        b.Property(c => c.RewardXP).IsRequired();
        b.Property(c => c.StartDate).IsRequired();
        b.Property(c => c.EndDate).IsRequired();
        b.Property(c => c.IsActive).HasDefaultValue(true);

        b.HasIndex(c => c.IsActive);
        b.HasIndex(c => new { c.StartDate, c.EndDate });
    }
}
