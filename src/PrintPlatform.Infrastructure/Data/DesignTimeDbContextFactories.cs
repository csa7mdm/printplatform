using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace PrintPlatform.Infrastructure.Data;

// Design-time factories let `dotnet ef migrations add` instantiate the contexts
// WITHOUT booting the API host (which requires full runtime config). The connection
// string here is design-time only; real runtime config comes from appsettings/DI.
internal static class DesignTimeConnection
{
    public const string Default =
        "Host=localhost;Port=5432;Database=printplatform;Username=postgres;Password=postgres";
}

public sealed class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
{
    public AppDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseNpgsql(DesignTimeConnection.Default)
            .Options;
        return new AppDbContext(options);
    }
}

public sealed class MarketplaceDbContextFactory : IDesignTimeDbContextFactory<MarketplaceDbContext>
{
    public MarketplaceDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<MarketplaceDbContext>()
            .UseNpgsql(DesignTimeConnection.Default)
            .Options;
        return new MarketplaceDbContext(options);
    }
}
