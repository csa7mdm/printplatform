using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Storage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PrintPlatform.Application;
using PrintPlatform.Application.Identity.Abstractions;
using PrintPlatform.Gamification;
using PrintPlatform.Gamification.Data;
using PrintPlatform.Infrastructure;
using PrintPlatform.Infrastructure.Data;
using PrintPlatform.Loyalty;
using PrintPlatform.Loyalty.Data;
using Testcontainers.PostgreSql;
using Xunit;

namespace PrintPlatform.Application.Tests.Integration;

/// <summary>
/// Host-less integration harness: builds a service provider from the application's
/// REAL registration extensions (AddApplication/AddInfrastructure/AddGamification/AddLoyalty)
/// against a Testcontainers Postgres. This exercises the full DI graph — including the
/// IModuleInstaller auto-registration scan and every cross-module dependency — without the
/// WebApplicationFactory HTTP relay (which disposes the host prematurely under minimal hosting).
/// </summary>
public abstract class IntegrationTestBase : IAsyncLifetime
{
    protected readonly PostgreSqlContainer DbContainer = new PostgreSqlBuilder()
        .WithImage("postgres:16-alpine")
        .Build();

    private ServiceProvider _provider = null!;
    protected IServiceProvider Services => _provider;

    public async Task InitializeAsync()
    {
        await DbContainer.StartAsync();
        var cs = DbContainer.GetConnectionString();

        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = cs,
                ["ConnectionStrings:HangfireConnection"] = cs,
                ["ConnectionStrings:Marketplace"] = cs,
                ["ConnectionStrings:Gamification"] = cs,
                ["ConnectionStrings:Loyalty"] = cs,
                ["Jwt:Secret"] = "integration-test-signing-key-not-secret-0123456789",
                ["Jwt:Issuer"] = "printplatform-tests",
                ["Jwt:Audience"] = "printplatform-tests",
            })
            .Build();

        var services = new ServiceCollection();
        services.AddSingleton<IConfiguration>(configuration);
        services.AddLogging();
        services.AddHttpContextAccessor();
        services.AddApplication();
        services.AddInfrastructure(configuration);
        services.AddGamification(configuration);
        services.AddLoyalty(configuration);
        services.AddScoped<ICurrentUserAccessor, TestCurrentUserAccessor>();

        _provider = services.BuildServiceProvider();

        // Create schema for the contexts the seeder writes to, then seed.
        // AppDbContext.EnsureCreated creates the database + its own tables. The other
        // contexts share that database, so EnsureCreated would no-op (DB already exists) —
        // create their tables explicitly instead.
        using var scope = _provider.CreateScope();
        var sp = scope.ServiceProvider;
        await sp.GetRequiredService<AppDbContext>().Database.EnsureCreatedAsync();
        await CreateTablesAsync(sp.GetRequiredService<GamificationDbContext>());
        await CreateTablesAsync(sp.GetRequiredService<LoyaltyDbContext>());

        await sp.GetRequiredService<DatabaseSeeder>().SeedAsync();
    }

    private static Task CreateTablesAsync(DbContext context)
        => context.Database.GetService<IRelationalDatabaseCreator>().CreateTablesAsync();

    public async Task DisposeAsync()
    {
        await _provider.DisposeAsync();
        await DbContainer.StopAsync();
    }

    private sealed class TestCurrentUserAccessor : ICurrentUserAccessor
    {
        public Guid? UserId => Guid.Parse("00000000-0000-0000-0000-000000000001");
        public string? IpAddress => "127.0.0.1";
    }
}
