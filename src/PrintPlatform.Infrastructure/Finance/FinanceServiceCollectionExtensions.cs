using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PrintPlatform.Application.Finance;
using PrintPlatform.Infrastructure.Data;

namespace PrintPlatform.Infrastructure.Finance;

public static class FinanceServiceCollectionExtensions
{
    public static IServiceCollection AddFinanceInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddScoped<IFinanceDbContext>(sp => sp.GetRequiredService<AppDbContext>());
        services.AddScoped<WeeklyPayoutBatchJob>();
        services.AddHostedService<FinanceHangfireRegistrationService>();

        return services;
    }
}
