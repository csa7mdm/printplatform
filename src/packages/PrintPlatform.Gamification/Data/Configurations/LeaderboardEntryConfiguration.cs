using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PrintPlatform.Gamification.Models;

namespace PrintPlatform.Gamification.Data.Configurations;

internal sealed class LeaderboardEntryConfiguration : IEntityTypeConfiguration<LeaderboardEntry>
{
    public void Configure(EntityTypeBuilder<LeaderboardEntry> b)
    {
        b.ToTable("gam_leaderboard_entries");

        b.HasKey(e => new { e.LeaderboardId, e.PlayerId });

        b.Property(e => e.Score).IsRequired();
        b.Property(e => e.Rank).IsRequired();
        b.Property(e => e.UpdatedAt).IsRequired();

        b.HasOne(e => e.Leaderboard)
         .WithMany(l => l.Entries)
         .HasForeignKey(e => e.LeaderboardId)
         .OnDelete(DeleteBehavior.Cascade);

        b.HasOne(e => e.Player)
         .WithMany()
         .HasForeignKey(e => e.PlayerId)
         .OnDelete(DeleteBehavior.Cascade);

        b.HasIndex(e => new { e.LeaderboardId, e.Rank });
    }
}
