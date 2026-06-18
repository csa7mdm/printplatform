using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PrintPlatform.Gamification.Models;

namespace PrintPlatform.Gamification.Data.Configurations;

internal sealed class StreakConfiguration : IEntityTypeConfiguration<Streak>
{
    public void Configure(EntityTypeBuilder<Streak> b)
    {
        b.ToTable("gam_streaks");

        b.HasKey(s => s.Id);
        b.Property(s => s.Id).UseIdentityColumn();

        b.Property(s => s.ActionType).IsRequired().HasMaxLength(100);
        b.Property(s => s.CurrentCount).HasDefaultValue(0);
        b.Property(s => s.BestCount).HasDefaultValue(0);
        b.Property(s => s.LastActionDate).IsRequired();

        b.HasOne(s => s.Player)
         .WithMany(p => p.Streaks)
         .HasForeignKey(s => s.PlayerId)
         .OnDelete(DeleteBehavior.Cascade);

        b.HasIndex(s => new { s.PlayerId, s.ActionType }).IsUnique();
    }
}
