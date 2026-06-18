using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PrintPlatform.Application.Orders.Quoting;

namespace PrintPlatform.Application.Orders;

/// <summary>
/// Registers Orders + Quoting application services. Kept separate from the root
/// <c>AddApplication</c> so module wiring stays isolated. Call from Program.cs.
/// </summary>
public static class OrdersApplicationModule
{
    public static IServiceCollection AddOrdersApplication(
        this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<QuotePricingOptions>(
            configuration.GetSection(QuotePricingOptions.SectionName));

        services.AddSingleton(TimeProvider.System);
        services.AddScoped<IQuotePricingCalculator, QuotePricingCalculator>();

        return services;
    }
}
