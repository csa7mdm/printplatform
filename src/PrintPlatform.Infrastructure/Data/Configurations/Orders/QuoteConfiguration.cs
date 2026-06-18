using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PrintPlatform.Domain.Orders;

namespace PrintPlatform.Infrastructure.Data.Configurations.Orders;

internal sealed class QuoteConfiguration : IEntityTypeConfiguration<Quote>
{
    public void Configure(EntityTypeBuilder<Quote> b)
    {
        b.ToTable("ord_quotes");
        b.HasKey(q => q.Id);

        b.Property(q => q.QuoteRequestId).IsRequired();
        // 1:1 with the quote request.
        b.HasIndex(q => q.QuoteRequestId).IsUnique();

        foreach (var prop in new[]
        {
            nameof(Quote.EstimatedWeightGrams), nameof(Quote.EstimatedPrintHours),
            nameof(Quote.MaterialCostEgp), nameof(Quote.DepreciationCostEgp),
            nameof(Quote.LaborCostEgp), nameof(Quote.PackagingCostEgp),
            nameof(Quote.FailureAllowanceEgp), nameof(Quote.TotalFulfillmentCostEgp),
            nameof(Quote.PlatformMarginEgp), nameof(Quote.CustomerPriceEgp),
            nameof(Quote.DesignServicePriceEgp), nameof(Quote.DeliveryEstimateEgp),
            nameof(Quote.TotalPriceEgp),
        })
        {
            b.Property(prop).HasPrecision(18, 2);
        }

        b.Property(q => q.OperatorNotes).HasMaxLength(2000);
        b.Property(q => q.Status).IsRequired().HasConversion<string>().HasMaxLength(20);

        b.Property(q => q.IsDeleted).HasDefaultValue(false);
        b.HasQueryFilter(q => !q.IsDeleted);
        b.Ignore(q => q.DomainEvents);
    }
}
