using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PrintPlatform.Gamification.Models;

namespace PrintPlatform.Gamification.Data.Configurations;

internal sealed class PlayerAchievementConfiguration : IEntityTypeConfiguration<PlayerAchievement>
{
    public void Configure(EntityTypeBuilder<PlayerAchievement> b)
    {
        b.ToTable("gam_player_achievements");

        b.HasKey(pa => new { pa.PlayerId, pa.AchievementId });

        b.Property(pa => pa.UnlockedAt).IsRequired();

        b.HasOne(pa => pa.Player)
         .WithMany(p => p.Achievements)
         .HasForeignKey(pa => pa.PlayerId)
         .OnDelete(DeleteBehavior.Cascade);

        b.HasOne(pa => pa.Achievement)
         .WithMany(a => a.PlayerAchievements)
         .HasForeignKey(pa => pa.AchievementId)
         .OnDelete(DeleteBehavior.Cascade);
    }
}
