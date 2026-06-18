using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PrintPlatform.Gamification.Models;

namespace PrintPlatform.Gamification.Data.Configurations;

internal sealed class GamificationPlayerConfiguration : IEntityTypeConfiguration<GamificationPlayer>
{
    public void Configure(EntityTypeBuilder<GamificationPlayer> b)
    {
        b.ToTable("gam_players");

        b.HasKey(p => p.Id);
        b.Property(p => p.Id).UseIdentityColumn();

        b.Property(p => p.ExternalUserId).IsRequired().HasMaxLength(256);
        b.HasIndex(p => p.ExternalUserId).IsUnique();

        b.Property(p => p.DisplayName).IsRequired().HasMaxLength(100);
        b.Property(p => p.TotalXP).HasDefaultValue(0);
        b.Property(p => p.CurrentLevel).HasDefaultValue(1);
        b.Property(p => p.CreatedAt).IsRequired();
    }
}
