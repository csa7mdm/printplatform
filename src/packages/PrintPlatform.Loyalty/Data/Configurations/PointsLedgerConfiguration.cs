using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PrintPlatform.Loyalty.Models;

namespace PrintPlatform.Loyalty.Data.Configurations;

internal sealed class PointsLedgerConfiguration : IEntityTypeConfiguration<PointsLedger>
{
    public void Configure(EntityTypeBuilder<PointsLedger> b)
    {
        b.ToTable("loy_ledger");

        b.HasKey(l => l.Id);
        b.Property(l => l.Id).UseIdentityColumn();

        b.Property(l => l.OrderReference).HasMaxLength(100);
        b.Property(l => l.Type).HasConversion<string>().HasMaxLength(20);

        b.HasOne(l => l.Member)
         .WithMany(m => m.LedgerEntries)
         .HasForeignKey(l => l.MemberId)
         .OnDelete(DeleteBehavior.Cascade);
    }
}
