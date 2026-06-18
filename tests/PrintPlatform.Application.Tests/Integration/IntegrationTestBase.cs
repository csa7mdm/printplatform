using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using PrintPlatform.Infrastructure.Data;
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

        var factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
                    if (descriptor != null) services.Remove(descriptor);

                    services.AddDbContext<AppDbContext>(options => options.UseNpgsql(DbContainer.GetConnectionString()));
                    
                    // Do the same for Gamification and Loyalty if they have separate connections in tests
                });
            });

        Client = factory.CreateClient();
        Services = factory.Services;

        using var scope = Services.CreateScope();
        var appDb = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await appDb.Database.EnsureCreatedAsync();
        
        var seeder = scope.ServiceProvider.GetRequiredService<DatabaseSeeder>();
        await seeder.SeedAsync();
    }

    public async Task DisposeAsync()
    {
        await DbContainer.StopAsync();
    }
}
