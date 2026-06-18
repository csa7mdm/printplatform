using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PrintPlatform.Domain.Orders;

namespace PrintPlatform.Infrastructure.Data.Configurations.Orders;

internal sealed class ModelFileConfiguration : IEntityTypeConfiguration<ModelFile>
{
    public void Configure(EntityTypeBuilder<ModelFile> b)
    {
        b.ToTable("ord_model_files");
        b.HasKey(m => m.Id);

        b.Property(m => m.CustomerUserId).IsRequired();
        b.Property(m => m.StorageKey).IsRequired().HasMaxLength(512);
        b.Property(m => m.OriginalFileName).IsRequired().HasMaxLength(260);
        b.Property(m => m.FileSizeBytes).IsRequired();
        b.Property(m => m.FileFormat).IsRequired().HasConversion<string>().HasMaxLength(20);
        b.Property(m => m.Status).IsRequired().HasConversion<string>().HasMaxLength(30);

        b.Property(m => m.EstimatedVolumeCC).HasPrecision(18, 4);
        b.Property(m => m.BoundingBoxX).HasPrecision(18, 4);
        b.Property(m => m.BoundingBoxY).HasPrecision(18, 4);
        b.Property(m => m.BoundingBoxZ).HasPrecision(18, 4);
        b.Property(m => m.EstimatedWeightGrams).HasPrecision(18, 4);
        b.Property(m => m.EstimatedPrintHours).HasPrecision(18, 4);
        b.Property(m => m.AnalysisNotes).HasMaxLength(2000);

        b.Property(m => m.IsDeleted).HasDefaultValue(false);
        b.HasQueryFilter(m => !m.IsDeleted);

        b.Ignore(m => m.DomainEvents);

        b.HasIndex(m => m.CustomerUserId);
        b.HasIndex(m => m.Status);
    }
}
