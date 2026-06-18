using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PrintPlatform.Domain.Orders;

namespace PrintPlatform.Infrastructure.Data.Configurations.Orders;

internal sealed class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> b)
    {
        b.ToTable("ord_orders");
        b.HasKey(o => o.Id);

        b.Property(o => o.CustomerUserId).IsRequired();
        b.Property(o => o.Status).IsRequired().HasConversion<string>().HasMaxLength(20);
        b.Property(o => o.PaymentMethod).IsRequired().HasConversion<string>().HasMaxLength(20);
        b.Property(o => o.PaymentStatus).IsRequired().HasConversion<string>().HasMaxLength(20);
        b.Property(o => o.PaymobOrderId).HasMaxLength(100);
        b.Property(o => o.ShippingAddressSnapshot).IsRequired().HasColumnType("jsonb");

        b.Property(o => o.SubtotalEgp).HasPrecision(18, 2);
        b.Property(o => o.DeliveryFeeEgp).HasPrecision(18, 2);
        b.Property(o => o.DiscountEgp).HasPrecision(18, 2);
        b.Property(o => o.TotalEgp).HasPrecision(18, 2);
        b.Property(o => o.LoyaltyDiscountEgp).HasPrecision(18, 2);
        b.Property(o => o.LoyaltyPointsUsed).IsRequired();

        // PostgreSQL optimistic concurrency via the system xmin column.
        b.Property(o => o.RowVersion).IsRowVersion().HasColumnName("xmin").HasColumnType("xid");

        b.Property(o => o.IsDeleted).HasDefaultValue(false);
        b.HasQueryFilter(o => !o.IsDeleted);
        b.Ignore(o => o.DomainEvents);

        b.HasMany(o => o.Items)
         .WithOne()
         .HasForeignKey(i => i.OrderId)
         .OnDelete(DeleteBehavior.Cascade);
        b.Metadata.FindNavigation(nameof(Order.Items))!
            .SetPropertyAccessMode(PropertyAccessMode.Field);

        b.HasIndex(o => o.CustomerUserId);
        b.HasIndex(o => o.Status);
        b.HasIndex(o => o.PaymobOrderId);
    }
}

internal sealed class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
{
    public void Configure(EntityTypeBuilder<OrderItem> b)
    {
        b.ToTable("ord_order_items");
        b.HasKey(i => i.Id);

        b.Property(i => i.OrderId).IsRequired();
        b.Property(i => i.QuoteId).IsRequired();
        b.Property(i => i.Quantity).IsRequired();
        b.Property(i => i.UnitPriceEgp).HasPrecision(18, 2);
        b.Property(i => i.TotalEgp).HasPrecision(18, 2);
        b.Property(i => i.ModelFileStorageKey).IsRequired().HasMaxLength(512);

        b.Property(i => i.IsDeleted).HasDefaultValue(false);
        b.HasQueryFilter(i => !i.IsDeleted);

        b.HasIndex(i => i.OrderId);
    }
}
