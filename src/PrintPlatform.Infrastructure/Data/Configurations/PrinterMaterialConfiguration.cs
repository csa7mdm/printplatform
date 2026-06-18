using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PrintPlatform.Domain.Marketplace;

namespace PrintPlatform.Infrastructure.Data.Configurations;

internal sealed class PrinterMaterialConfiguration : IEntityTypeConfiguration<PrinterMaterial>
{
    public void Configure(EntityTypeBuilder<PrinterMaterial> b)
    {
        b.ToTable("mkt_printer_materials");

        b.HasKey(pm => pm.Id);
        b.Property(pm => pm.Id).ValueGeneratedNever();

        b.Property(pm => pm.PrinterId).IsRequired();
        b.Property(pm => pm.MaterialOptionId).IsRequired();
        b.Property(pm => pm.MaxQuality).HasConversion<string>().HasMaxLength(20).IsRequired();
        b.Property(pm => pm.IsDefault).IsRequired();

        b.Property(pm => pm.CreatedAt).IsRequired();
        b.Property(pm => pm.UpdatedAt).IsRequired();
        b.Property(pm => pm.IsDeleted).IsRequired();

        b.HasOne(pm => pm.MaterialOption)
         .WithMany()
         .HasForeignKey(pm => pm.MaterialOptionId)
         .OnDelete(DeleteBehavior.Restrict);

        b.HasIndex(pm => new { pm.PrinterId, pm.MaterialOptionId }).IsUnique();
    }
}
