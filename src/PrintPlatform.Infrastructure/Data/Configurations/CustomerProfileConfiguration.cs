using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PrintPlatform.Domain.Identity;

namespace PrintPlatform.Infrastructure.Data.Configurations;

public sealed class CustomerProfileConfiguration : IEntityTypeConfiguration<CustomerProfile>
{
    public void Configure(EntityTypeBuilder<CustomerProfile> b)
    {
        b.ToTable("customer_profiles");
        b.HasKey(p => p.Id);
        b.Property(p => p.Id).ValueGeneratedNever();

        b.Property(p => p.UserId).IsRequired();
        b.HasIndex(p => p.UserId).IsUnique();

        // 1:1 link to the domain User.
        b.HasOne<User>()
            .WithOne()
            .HasForeignKey<CustomerProfile>(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        b.Property(p => p.LoyaltyMemberId).HasMaxLength(64);
        b.Property(p => p.PreferredPaymentMethod).HasMaxLength(40);

        b.Property(p => p.RowVersion)
            .IsRowVersion()
            .HasColumnName("xmin")
            .HasColumnType("xid");

        b.HasQueryFilter(p => !p.IsDeleted);
    }
}
