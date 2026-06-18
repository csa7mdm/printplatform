using Microsoft.EntityFrameworkCore;
using PrintPlatform.Gamification.Data;

namespace PrintPlatform.Gamification.Tests.Helpers;

/// <summary>Creates an isolated in-memory <see cref="GamificationDbContext"/> per test.</summary>
internal static class GamificationDbContextFactory
{
    public static GamificationDbContext Create(string? dbName = null)
    {
        var options = new DbContextOptionsBuilder<GamificationDbContext>()
            .UseInMemoryDatabase(dbName ?? Guid.NewGuid().ToString())
            .Options;

        var ctx = new GamificationDbContext(options);
        ctx.Database.EnsureCreated();
        return ctx;
    }
}
