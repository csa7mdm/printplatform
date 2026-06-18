using Microsoft.EntityFrameworkCore;
using PrintPlatform.Loyalty.Data.Configurations;
using PrintPlatform.Loyalty.Models;

namespace PrintPlatform.Loyalty.Data;

/// <summary>
/// Self-contained EF Core context for the Loyalty package.
/// All tables are prefixed with <c>loy_</c> to avoid naming collisions in the host DB.
/// </summary>
public sealed class LoyaltyDbContext : DbContext
{
    public LoyaltyDbContext(DbContextOptions<LoyaltyDbContext> options)
        : base(options) { }

    public DbSet<LoyaltyMember> Members       { get; set; } = null!;
    public DbSet<LoyaltyTier>   Tiers         { get; set; } = null!;
    public DbSet<PointsLedger>  LedgerEntries { get; set; } = null!;
    public DbSet<Redemption>    Redemptions   { get; set; } = null!;
    public DbSet<Referral>      Referrals     { get; set; } = null!;
    public DbSet<Reward>        Rewards       { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new LoyaltyMemberConfiguration());
        modelBuilder.ApplyConfiguration(new LoyaltyTierConfiguration());
        modelBuilder.ApplyConfiguration(new PointsLedgerConfiguration());
        modelBuilder.ApplyConfiguration(new RedemptionConfiguration());
        modelBuilder.ApplyConfiguration(new ReferralConfiguration());
        modelBuilder.ApplyConfiguration(new RewardConfiguration());
    }
}
