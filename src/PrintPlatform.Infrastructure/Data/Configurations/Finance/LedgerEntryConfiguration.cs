using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PrintPlatform.Domain.Finance;

namespace PrintPlatform.Infrastructure.Data.Configurations.Finance;

internal sealed class LedgerEntryConfiguration : IEntityTypeConfiguration<LedgerEntry>
{
    public void Configure(EntityTypeBuilder<LedgerEntry> b)
    {
        b.ToTable("fin_ledger_entries");
        b.HasKey(e => e.Id);

        b.Property(e => e.EntryType).IsRequired().HasConversion<string>().HasMaxLength(40);
        b.Property(e => e.AmountEgp).IsRequired().HasPrecision(18, 2);
        b.Property(e => e.Direction).IsRequired().HasConversion<string>().HasMaxLength(8);
        b.Property(e => e.Description).IsRequired().HasMaxLength(500);
        b.Property(e => e.ExternalReference).HasMaxLength(200);
        b.Property(e => e.CreatedAt).IsRequired();

        b.Property(e => e.IsDeleted).HasDefaultValue(false);
        b.Ignore(e => e.DomainEvents);
        b.Ignore(e => e.UpdatedAt);

        b.HasIndex(e => e.CreatedAt);
        b.HasIndex(e => e.EntryType);
        b.HasIndex(e => e.RelatedOrderId);
        b.HasIndex(e => e.RelatedJobAssignmentId);
        b.HasIndex(e => e.RelatedPayoutId);
        b.HasIndex(e => e.PrinterOwnerUserId);
        b.HasIndex(e => e.CustomerUserId);

        b.Metadata.SetComment("Append-only finance ledger. Application handlers only add rows; entries are never updated for business corrections.");
    }
}
