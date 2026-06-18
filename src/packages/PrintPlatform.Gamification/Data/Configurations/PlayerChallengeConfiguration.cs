using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PrintPlatform.Gamification.Models;

namespace PrintPlatform.Gamification.Data.Configurations;

internal sealed class PlayerChallengeConfiguration : IEntityTypeConfiguration<PlayerChallenge>
{
    public void Configure(EntityTypeBuilder<PlayerChallenge> b)
    {
        b.ToTable("gam_player_challenges");

        b.HasKey(pc => new { pc.PlayerId, pc.ChallengeId });

        b.Property(pc => pc.Progress).HasDefaultValue(0);

        b.HasOne(pc => pc.Player)
         .WithMany(p => p.Challenges)
         .HasForeignKey(pc => pc.PlayerId)
         .OnDelete(DeleteBehavior.Cascade);

        b.HasOne(pc => pc.Challenge)
         .WithMany(c => c.PlayerChallenges)
         .HasForeignKey(pc => pc.ChallengeId)
         .OnDelete(DeleteBehavior.Cascade);
    }
}
