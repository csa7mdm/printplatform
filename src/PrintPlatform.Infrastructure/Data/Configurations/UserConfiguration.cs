using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PrintPlatform.Domain.Identity;

namespace PrintPlatform.Infrastructure.Data.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> b)
    {
        b.ToTable("users");
        b.HasKey(u => u.Id);
        b.Property(u => u.Id).ValueGeneratedNever();

        b.Property(u => u.Email).IsRequired().HasMaxLength(256);
        b.HasIndex(u => u.Email).IsUnique();

        b.Property(u => u.PhoneNumber).IsRequired().HasMaxLength(20);
        b.HasIndex(u => u.PhoneNumber).IsUnique();

        b.Property(u => u.FullNameAr).IsRequired().HasMaxLength(200);
        b.Property(u => u.FullNameEn).IsRequired().HasMaxLength(200);

        b.Property(u => u.Role).HasConversion<string>().HasMaxLength(20);
        b.Property(u => u.Lang).HasConversion<string>().HasMaxLength(10);

        b.Property(u => u.IsActive);
        b.Property(u => u.IsVerified);
        b.Property(u => u.IsDeleted);
        b.Property(u => u.CreatedAt);
        b.Property(u => u.UpdatedAt);

        // PostgreSQL optimistic concurrency via system column xmin.
        b.Property(u => u.RowVersion)
            .IsRowVersion()
            .HasColumnName("xmin")
            .HasColumnType("xid");

        // Refresh tokens as an owned collection (separate table).
        b.OwnsMany(u => u.RefreshTokens, rt =>
        {
            rt.ToTable("refresh_tokens");
            rt.WithOwner().HasForeignKey("UserId");
            rt.HasKey(t => t.Id);
            rt.Property(t => t.Id).ValueGeneratedNever();
            rt.Property(t => t.TokenHash).IsRequired().HasMaxLength(128);
            rt.HasIndex(t => t.TokenHash);
            rt.Property(t => t.CreatedByIp).HasMaxLength(64);
            rt.Property(t => t.RevokedByIp).HasMaxLength(64);
            rt.Property(t => t.ReplacedByTokenHash).HasMaxLength(128);
            rt.Property(t => t.CreatedAt);
            rt.Property(t => t.ExpiresAt);
            rt.Property(t => t.RevokedAt);
        });

        b.Navigation(u => u.RefreshTokens).UsePropertyAccessMode(PropertyAccessMode.Field);

        // Global soft-delete filter.
        b.HasQueryFilter(u => !u.IsDeleted);
    }
}
