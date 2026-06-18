using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PrintPlatform.Gamification.Models;

namespace PrintPlatform.Gamification.Data.Configurations;

internal sealed class AchievementConfiguration : IEntityTypeConfiguration<Achievement>
{
    public void Configure(EntityTypeBuilder<Achievement> b)
    {
        b.ToTable("gam_achievements");

        b.HasKey(a => a.Id);
        b.Property(a => a.Id).UseIdentityColumn();

        b.Property(a => a.Name).IsRequired().HasMaxLength(100);
        b.Property(a => a.Description).HasMaxLength(500);
        b.Property(a => a.IconSlug).HasMaxLength(100);
        b.Property(a => a.Category).HasMaxLength(50);
        b.Property(a => a.CriteriaJson).HasColumnType("jsonb").IsRequired();
        b.Property(a => a.BonusXP).HasDefaultValue(0);

        b.HasIndex(a => a.Category);
    }
}
