using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PrintPlatform.Domain.Identity;

namespace PrintPlatform.Infrastructure.Data.Configurations;

public sealed class PrinterOwnerProfileConfiguration : IEntityTypeConfiguration<PrinterOwnerProfile>
{
    public void Configure(EntityTypeBuilder<PrinterOwnerProfile> b)
    {
        b.ToTable("printer_owner_profiles");
        b.HasKey(p => p.Id);
        b.Property(p => p.Id).ValueGeneratedNever();

        b.Property(p => p.UserId).IsRequired();
        b.HasIndex(p => p.UserId).IsUnique();

        b.HasOne<User>()
            .WithOne()
            .HasForeignKey<PrinterOwnerProfile>(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Encrypted at the application layer; stored as opaque ciphertext.
        b.Property(p => p.NationalId).IsRequired().HasMaxLength(512);
        b.Property(p => p.BankAccountDetails).HasMaxLength(1024);

        b.Property(p => p.BusinessName).IsRequired().HasMaxLength(200);
        b.Property(p => p.InstapayNumber).HasMaxLength(20);
        b.Property(p => p.GamificationPlayerId).HasMaxLength(64);

        b.Property(p => p.CertificationLevel).HasConversion<string>().HasMaxLength(20);
        b.Property(p => p.CertificationScore).HasPrecision(5, 2);
        b.Property(p => p.IsAvailable);
        b.Property(p => p.OnboardingCompletedAt);

        b.Property(p => p.RowVersion)
            .IsRowVersion()
            .HasColumnName("xmin")
            .HasColumnType("xid");

        b.HasQueryFilter(p => !p.IsDeleted);
    }
}
