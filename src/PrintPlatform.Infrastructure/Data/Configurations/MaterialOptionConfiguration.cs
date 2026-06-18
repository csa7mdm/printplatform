using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PrintPlatform.Domain.Marketplace;

namespace PrintPlatform.Infrastructure.Data.Configurations;

internal sealed class MaterialOptionConfiguration : IEntityTypeConfiguration<MaterialOption>
{
    public void Configure(EntityTypeBuilder<MaterialOption> b)
    {
        b.ToTable("mkt_material_options");

        b.HasKey(m => m.Id);
        b.Property(m => m.Id).ValueGeneratedNever();

        b.Property(m => m.MaterialType).HasConversion<string>().HasMaxLength(30).IsRequired();
        b.Property(m => m.ColorName).IsRequired().HasMaxLength(100);
        b.Property(m => m.ColorHex).HasMaxLength(9);

        b.Property(m => m.PricePerGram).HasPrecision(10, 4).IsRequired();
        b.Property(m => m.OwnerCostPerGram).HasPrecision(10, 4).IsRequired();

        b.Property(m => m.PropertiesJson).HasColumnType("jsonb").IsRequired();
        b.Property(m => m.IsActive).IsRequired();

        b.Property(m => m.CreatedAt).IsRequired();
        b.Property(m => m.UpdatedAt).IsRequired();
        b.Property(m => m.IsDeleted).IsRequired();

        b.HasQueryFilter(m => !m.IsDeleted);

        b.HasIndex(m => m.MaterialType);
        b.HasIndex(m => m.IsActive);
    }
}
