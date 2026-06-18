using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PrintPlatform.Gamification.Models;

namespace PrintPlatform.Gamification.Data.Configurations;

internal sealed class LeaderboardConfiguration : IEntityTypeConfiguration<Leaderboard>
{
    public void Configure(EntityTypeBuilder<Leaderboard> b)
    {
        b.ToTable("gam_leaderboards");

        b.HasKey(l => l.Id);
        b.Property(l => l.Id).UseIdentityColumn();

        b.Property(l => l.Name).IsRequired().HasMaxLength(100);
        b.Property(l => l.Period).IsRequired().HasConversion<string>();
        b.Property(l => l.Type).IsRequired().HasConversion<string>();

        b.HasIndex(l => new { l.Period, l.Type });
    }
}
