using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace PrintPlatform.Gamification.Data;

/// <summary>Design-time factory so `dotnet ef migrations add` works without a host.</summary>
public sealed class GamificationDbContextFactory : IDesignTimeDbContextFactory<GamificationDbContext>
{
    public GamificationDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<GamificationDbContext>()
            .UseNpgsql("Host=localhost;Port=5432;Database=printplatform;Username=postgres;Password=postgres")
            .Options;
        return new GamificationDbContext(options);
    }
}
