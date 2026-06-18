using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PrintPlatform.Domain.Orders;

namespace PrintPlatform.Infrastructure.Data.Configurations.Orders;

internal sealed class QuoteRequestConfiguration : IEntityTypeConfiguration<QuoteRequest>
{
    public void Configure(EntityTypeBuilder<QuoteRequest> b)
    {
        b.ToTable("ord_quote_requests");
        b.HasKey(q => q.Id);

        b.Property(q => q.ModelFileId).IsRequired();
        b.Property(q => q.CustomerUserId).IsRequired();
        b.Property(q => q.MaterialOptionId).IsRequired();
        b.Property(q => q.QualityPreset).IsRequired().HasConversion<string>().HasMaxLength(20);
        b.Property(q => q.LayerHeightMm).HasPrecision(6, 3);
        b.Property(q => q.InfillPercent).IsRequired();
        b.Property(q => q.Quantity).IsRequired();
        b.Property(q => q.IncludeDesignService).IsRequired();
        b.Property(q => q.Status).IsRequired().HasConversion<string>().HasMaxLength(20);

        b.Property(q => q.IsDeleted).HasDefaultValue(false);
        b.HasQueryFilter(q => !q.IsDeleted);
        b.Ignore(q => q.DomainEvents);

        b.HasIndex(q => q.CustomerUserId);
        b.HasIndex(q => q.Status);
        b.HasIndex(q => q.ModelFileId);
    }
}
