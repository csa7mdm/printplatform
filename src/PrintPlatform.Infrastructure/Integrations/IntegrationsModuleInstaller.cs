using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PrintPlatform.Infrastructure.Modules;

namespace PrintPlatform.Infrastructure.Integrations;

/// <summary>Auto-discovered installer for external integrations (MinIO storage, WhatsApp).</summary>
public sealed class IntegrationsModuleInstaller : IModuleInstaller
{
    public void Install(IServiceCollection services, IConfiguration configuration)
        => services.AddIntegrationsInfrastructure(configuration);
}
