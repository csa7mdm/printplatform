using Microsoft.EntityFrameworkCore;
using PrintPlatform.Gamification.Data.Configurations;
using PrintPlatform.Gamification.Models;

namespace PrintPlatform.Gamification.Data;

/// <summary>
/// Self-contained EF Core context for the Gamification package.
/// All tables are prefixed with <c>gam_</c> to avoid naming collisions in the host DB.
/// </summary>
public sealed class GamificationDbContext : DbContext
{
    public GamificationDbContext(DbContextOptions<GamificationDbContext> options)
        : base(options) { }

    public DbSet<GamificationPlayer> Players          { get; set; } = null!;
    public DbSet<PointTransaction>   Transactions     { get; set; } = null!;
    public DbSet<Achievement>        Achievements     { get; set; } = null!;
    public DbSet<PlayerAchievement>  PlayerAchievements { get; set; } = null!;
    public DbSet<Level>              Levels           { get; set; } = null!;
    public DbSet<Challenge>          Challenges       { get; set; } = null!;
    public DbSet<PlayerChallenge>    PlayerChallenges { get; set; } = null!;
    public DbSet<Streak>             Streaks          { get; set; } = null!;
    public DbSet<Leaderboard>        Leaderboards     { get; set; } = null!;
    public DbSet<LeaderboardEntry>   LeaderboardEntries { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfiguration(new GamificationPlayerConfiguration());
        modelBuilder.ApplyConfiguration(new PointTransactionConfiguration());
        modelBuilder.ApplyConfiguration(new AchievementConfiguration());
        modelBuilder.ApplyConfiguration(new PlayerAchievementConfiguration());
        modelBuilder.ApplyConfiguration(new LevelConfiguration());
        modelBuilder.ApplyConfiguration(new ChallengeConfiguration());
        modelBuilder.ApplyConfiguration(new PlayerChallengeConfiguration());
        modelBuilder.ApplyConfiguration(new StreakConfiguration());
        modelBuilder.ApplyConfiguration(new LeaderboardConfiguration());
        modelBuilder.ApplyConfiguration(new LeaderboardEntryConfiguration());
    }
}
