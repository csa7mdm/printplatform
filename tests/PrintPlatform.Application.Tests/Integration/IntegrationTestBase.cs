using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PrintPlatform.Gamification.Data;
using PrintPlatform.Infrastructure.Data;
using PrintPlatform.Loyalty.Data;
using Testcontainers.PostgreSql;
using Xunit;

namespace PrintPlatform.Application.Tests.Integration;

public abstract class IntegrationTestBase : IAsyncLifetime
{
    protected readonly PostgreSqlContainer DbContainer = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .Build();

    protected HttpClient Client { get; private set; } = null!;
    protected IServiceProvider Services { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        await DbContainer.StartAsync();
        var connectionString = DbContainer.GetConnectionString();

        var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.UseEnvironment("Testing");

                // Point EVERY connection (all DbContexts + Hangfire) at the test container
                // so the app boots fully isolated from any local Postgres.
                builder.ConfigureAppConfiguration((_, config) =>
                {
                    config.AddInMemoryCollection(new Dictionary<string, string?>
                    {
                        ["ConnectionStrings:DefaultConnection"] = connectionString,
                        ["ConnectionStrings:HangfireConnection"] = connectionString,
                        ["ConnectionStrings:Marketplace"] = connectionString,
                        ["ConnectionStrings:Gamification"] = connectionString,
                        ["ConnectionStrings:Loyalty"] = connectionString,
                    });
                });
            });

        Client = factory.CreateClient();
        Services = factory.Services;

        // Create schema for every context, then seed reference data.
        using var scope = Services.CreateScope();
        var sp = scope.ServiceProvider;
        await sp.GetRequiredService<AppDbContext>().Database.EnsureCreatedAsync();
        await sp.GetRequiredService<MarketplaceDbContext>().Database.EnsureCreatedAsync();
        await sp.GetRequiredService<GamificationDbContext>().Database.EnsureCreatedAsync();
        await sp.GetRequiredService<LoyaltyDbContext>().Database.EnsureCreatedAsync();

        await sp.GetRequiredService<DatabaseSeeder>().SeedAsync();
    }

    public async Task DisposeAsync()
    {
        await DbContainer.StopAsync();
    }
}
