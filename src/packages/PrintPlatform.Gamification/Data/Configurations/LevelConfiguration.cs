using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PrintPlatform.Gamification.Models;

namespace PrintPlatform.Gamification.Data.Configurations;

internal sealed class LevelConfiguration : IEntityTypeConfiguration<Level>
{
    public void Configure(EntityTypeBuilder<Level> b)
    {
        b.ToTable("gam_levels");

        b.HasKey(l => l.Tier);
        b.Property(l => l.Tier).ValueGeneratedNever();

        b.Property(l => l.Name).IsRequired().HasMaxLength(50);
        b.Property(l => l.MinXP).IsRequired();
        b.Property(l => l.MaxXP).IsRequired();
        b.Property(l => l.BenefitsJson).HasColumnType("jsonb").IsRequired();
    }
}
