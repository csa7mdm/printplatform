using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace PrintPlatform.Loyalty.Data;

/// <summary>Design-time factory so `dotnet ef migrations add` works without a host.</summary>
public sealed class LoyaltyDbContextFactory : IDesignTimeDbContextFactory<LoyaltyDbContext>
{
    public LoyaltyDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<LoyaltyDbContext>()
            .UseNpgsql("Host=localhost;Port=5432;Database=printplatform;Username=postgres;Password=postgres")
            .Options;
        return new LoyaltyDbContext(options);
    }
}
