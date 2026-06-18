using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PrintPlatform.Domain.Marketplace;

namespace PrintPlatform.Infrastructure.Data.Configurations;

internal sealed class PrinterConfiguration : IEntityTypeConfiguration<Printer>
{
    public void Configure(EntityTypeBuilder<Printer> b)
    {
        b.ToTable("mkt_printers");

        b.HasKey(p => p.Id);
        b.Property(p => p.Id).ValueGeneratedNever();

        b.Property(p => p.PrinterOwnerProfileId).IsRequired();
        b.Property(p => p.Brand).IsRequired().HasMaxLength(100);
        b.Property(p => p.Model).IsRequired().HasMaxLength(100);

        b.Property(p => p.TechnologyType).HasConversion<string>().HasMaxLength(20).IsRequired();
        b.Property(p => p.Status).HasConversion<string>().HasMaxLength(30).IsRequired();

        b.Property(p => p.BuildVolumeX).IsRequired();
        b.Property(p => p.BuildVolumeY).IsRequired();
        b.Property(p => p.BuildVolumeZ).IsRequired();
        b.Property(p => p.NozzleDiameter).HasPrecision(5, 2);

        b.Property(p => p.SupportedMaterialsJson).HasColumnType("jsonb").IsRequired();
        b.Property(p => p.MaxLayerHeight).HasPrecision(5, 3);
        b.Property(p => p.MinLayerHeight).HasPrecision(5, 3);

        b.Property(p => p.AreaDistrict).IsRequired().HasMaxLength(100);
        b.Property(p => p.AreaCity).IsRequired().HasMaxLength(100);
        b.Property(p => p.IsDeliverable).IsRequired();

        b.Property(p => p.CertificationLevel).IsRequired();
        b.Property(p => p.MaxConcurrentJobs).IsRequired().HasDefaultValue(1);

        b.Property(p => p.CreatedAt).IsRequired();
        b.Property(p => p.UpdatedAt).IsRequired();
        b.Property(p => p.IsDeleted).IsRequired();

        // Soft-delete global filter.
        b.HasQueryFilter(p => !p.IsDeleted);

        b.HasMany(p => p.Materials)
         .WithOne()
         .HasForeignKey(pm => pm.PrinterId)
         .OnDelete(DeleteBehavior.Cascade);

        b.Navigation(p => p.Materials).UsePropertyAccessMode(PropertyAccessMode.Field);

        b.HasIndex(p => p.PrinterOwnerProfileId);
        b.HasIndex(p => p.Status);
        b.HasIndex(p => p.AreaDistrict);
    }
}
