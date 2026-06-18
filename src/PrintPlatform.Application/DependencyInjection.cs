using FluentValidation;
using Mapster;
using MapsterMapper;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using PrintPlatform.Application.Events;
using PrintPlatform.Domain.Shared;
using System.Reflection;

namespace PrintPlatform.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        var assembly = Assembly.GetExecutingAssembly();

        // MediatR — scans this assembly for handlers, pipeline behaviours, etc.
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(assembly));

        // FluentValidation — auto-register all validators in this assembly.
        services.AddValidatorsFromAssembly(assembly);

        // Mapster
        var config = TypeAdapterConfig.GlobalSettings;
        config.Scan(assembly);
        services.AddSingleton(config);
        services.AddScoped<IMapper, ServiceMapper>();

        // Internal event bus (bridges Domain events → MediatR notifications).
        services.AddScoped<IDomainEventDispatcher, InternalEventBus>();

        // Marketplace ranking (pure logic; depends only on IDistrictDistanceCalculator).
        services.AddScoped<Marketplace.PrinterRankingService>();

        return services;
    }
}
