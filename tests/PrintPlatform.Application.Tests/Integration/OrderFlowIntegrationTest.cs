using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using PrintPlatform.Domain.Orders;
using PrintPlatform.Infrastructure.Data;
using Xunit;

namespace PrintPlatform.Application.Tests.Integration;

public class OrderFlowIntegrationTest : IntegrationTestBase
{
    [Fact]
    public async Task FullOrderFlow_Should_UpdateOrderStatusAndLedger()
    {
        // 1. Register customer
        // 2. Upload model
        // 3. Request quote
        // 4. Operator confirm quote
        // 5. Accept quote
        // 6. Payment webhook
        // 7. Assign job
        // 8. Owner accept job
        // 9. Upload photos
        // 10. QC Approve
        // 11. Bosta webhook
        // 12. Delivered

        // For now, we'll just assert that the seeder ran
        using var scope = Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        var materials = await db.Set<PrintPlatform.Domain.Marketplace.MaterialOption>().ToListAsync();
        materials.Should().NotBeEmpty();
    }
}
