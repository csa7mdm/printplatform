using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using PrintPlatform.Application.Abstractions;
using PrintPlatform.Application.Identity.Abstractions;
using PrintPlatform.Infrastructure.Data;
using PrintPlatform.Infrastructure.Dispatch;
using PrintPlatform.Infrastructure.Identity;
using PrintPlatform.Infrastructure.Integrations;
using PrintPlatform.Infrastructure.Marketplace;
using PrintPlatform.Infrastructure.Modules;
using PrintPlatform.Infrastructure.Notifications;
using PrintPlatform.Infrastructure.Shipping;

namespace PrintPlatform.Infrastructure;

public static class DependencyInjection
{
    /// <summary>
    /// Registers EF Core DbContext, ASP.NET Core Identity, the Identity module
    /// services (JWT, refresh tokens, OTP, encryption), data protection and
    /// HTTP resilience policies into the DI container.
    /// </summary>
    public static IServiceCollection AddInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // -- Database ----------------------------------------------------------
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("ConnectionStrings:DefaultConnection is not configured.");

        services.AddDbContext<AppDbContext>(opts =>
            opts.UseNpgsql(connectionString, npgsql =>
                npgsql.MigrationsAssembly(typeof(AppDbContext).Assembly.FullName)));

        // -- ASP.NET Core Identity --------------------------------------------
        services
            .AddIdentityCore<ApplicationUser>(opts =>
            {
                opts.User.RequireUniqueEmail = true;
                opts.Password.RequiredLength = 8;
                opts.Password.RequireNonAlphanumeric = false;
                opts.Lockout.MaxFailedAccessAttempts = 5;
                opts.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
                opts.SignIn.RequireConfirmedPhoneNumber = false;
            })
            .AddRoles<ApplicationRole>()
            .AddEntityFrameworkStores<AppDbContext>();

        // -- Identity module options & services -------------------------------
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));

        services.AddScoped<IIdentityRepository, IdentityRepository>();
        services.AddScoped<IUserCredentialStore, UserCredentialStore>();
        services.AddSingleton<IJwtTokenGenerator, JwtTokenGenerator>();
        services.AddSingleton<IRefreshTokenFactory, RefreshTokenFactory>();
        services.AddSingleton<IFieldEncryptor, FieldEncryptor>();
        services.AddScoped<IOtpSender, WhatsAppOtpSender>();

        // -- Data Protection --------------------------------------------------
        services.AddDataProtection()
            .SetApplicationName("PrintPlatform");

        // -- Vertical-slice modules (auto-discovered) -------------------------
        // Each module registers itself via an IModuleInstaller in its own folder.
        // Adding a module = adding an installer class; this file never changes.
        // See PrintPlatform.Infrastructure.Modules.IModuleInstaller + ORCHESTRATION.md.
        services.InstallModules(configuration);

        services.AddScoped<DatabaseSeeder>();

        return services;
    }
}
