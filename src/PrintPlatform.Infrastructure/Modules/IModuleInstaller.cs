using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace PrintPlatform.Infrastructure.Modules;

/// <summary>
/// Implemented by each vertical-slice module to register its own services.
/// Implementations are discovered and invoked automatically by
/// <see cref="ModuleInstallerExtensions.InstallModules"/> during
/// <c>AddInfrastructure</c> — so a new module never edits a shared DI file.
///
/// CONTRACT: a module adds ONE class implementing this interface inside its own
/// Infrastructure folder, with a public parameterless constructor. It must touch
/// no other module's registrations.
/// </summary>
public interface IModuleInstaller
{
    void Install(IServiceCollection services, IConfiguration configuration);
}

/// <summary>
/// Scans the Infrastructure assembly for every <see cref="IModuleInstaller"/> and
/// invokes it. This is the mechanism that keeps module registration conflict-free:
/// adding a module = adding an installer class, with zero edits to
/// <c>DependencyInjection.AddInfrastructure</c>.
/// </summary>
public static class ModuleInstallerExtensions
{
    public static IServiceCollection InstallModules(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var installers = typeof(ModuleInstallerExtensions).Assembly
            .GetTypes()
            .Where(t => typeof(IModuleInstaller).IsAssignableFrom(t)
                        && t is { IsAbstract: false, IsInterface: false })
            .Select(t => (IModuleInstaller)Activator.CreateInstance(t)!);

        foreach (var installer in installers)
            installer.Install(services, configuration);

        return services;
    }
}
