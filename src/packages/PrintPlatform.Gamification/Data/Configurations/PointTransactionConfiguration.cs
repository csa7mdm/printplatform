using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PrintPlatform.Gamification.Models;

namespace PrintPlatform.Gamification.Data.Configurations;

internal sealed class PointTransactionConfiguration : IEntityTypeConfiguration<PointTransaction>
{
    public void Configure(EntityTypeBuilder<PointTransaction> b)
    {
        b.ToTable("gam_point_transactions");

        b.HasKey(t => t.Id);
        b.Property(t => t.Id).UseIdentityColumn();

        b.Property(t => t.Action).IsRequired().HasMaxLength(100);
        b.Property(t => t.Points).IsRequired();
        b.Property(t => t.MetadataJson).HasColumnType("jsonb");
        b.Property(t => t.CreatedAt).IsRequired();

        b.HasOne(t => t.Player)
         .WithMany(p => p.Transactions)
         .HasForeignKey(t => t.PlayerId)
         .OnDelete(DeleteBehavior.Cascade);

        b.HasIndex(t => t.PlayerId);
        b.HasIndex(t => t.CreatedAt);
    }
}
