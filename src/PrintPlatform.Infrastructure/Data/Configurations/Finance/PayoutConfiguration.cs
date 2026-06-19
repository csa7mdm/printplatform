using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PrintPlatform.Domain.Finance;
using PrintPlatform.Infrastructure.Data.Configurations;

namespace PrintPlatform.Infrastructure.Data.Configurations.Finance;

internal sealed class PayoutConfiguration : IEntityTypeConfiguration<Payout>
{
    public void Configure(EntityTypeBuilder<Payout> b)
    {
        b.ToTable("fin_payouts");
        b.HasKey(p => p.Id);

        b.Property(p => p.PrinterOwnerUserId).IsRequired();
        b.Property(p => p.PeriodStart).IsRequired();
        b.Property(p => p.PeriodEnd).IsRequired();
        b.Property(p => p.GrossAmountEgp).IsRequired().HasPrecision(18, 2);
        b.Property(p => p.PlatformFeeEgp).IsRequired().HasPrecision(18, 2);
        b.Property(p => p.NetAmountEgp).IsRequired().HasPrecision(18, 2);
        b.Property(p => p.Status).IsRequired().HasConversion<string>().HasMaxLength(30);
        b.Property(p => p.PaymentMethod).IsRequired().HasConversion<string>().HasMaxLength(30);
        b.Property(p => p.ExternalReference).HasMaxLength(200);
        b.Property<List<Guid>>("_jobAssignmentIds")
            .HasColumnName("JobAssignmentIds")
            .HasJsonConversion();
        b.Ignore(p => p.JobAssignmentIds);
        b.Property(p => p.CreatedAt).IsRequired();

        b.Property(p => p.IsDeleted).HasDefaultValue(false);
        b.Ignore(p => p.DomainEvents);

        b.HasIndex(p => p.PrinterOwnerUserId);
        b.HasIndex(p => p.Status);
        b.HasIndex(p => new { p.PeriodStart, p.PeriodEnd });
    }
}
