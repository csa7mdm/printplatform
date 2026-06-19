using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PrintPlatform.Application.Abstractions;
using PrintPlatform.Infrastructure.Modules;
using PrintPlatform.Infrastructure.Shipping;

namespace PrintPlatform.Infrastructure.Dispatch;

/// <summary>Auto-discovered installer for the Dispatch (job-routing + QC) module.</summary>
public sealed class DispatchModuleInstaller : IModuleInstaller
{
    public void Install(IServiceCollection services, IConfiguration configuration)
    {
        services.AddDispatchInfrastructure();
        services.AddScoped<IShippingService, BostaShippingService>();
    }
}
