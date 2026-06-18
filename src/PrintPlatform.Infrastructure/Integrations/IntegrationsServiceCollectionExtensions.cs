using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using PrintPlatform.Application.Abstractions;
using PrintPlatform.Infrastructure.Notifications;
using PrintPlatform.Infrastructure.Storage;
using System;
using System.Net;

namespace PrintPlatform.Infrastructure.Integrations;

public static class IntegrationsServiceCollectionExtensions
{
    public static IServiceCollection AddIntegrationsInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        // 1. File Storage (MinIO)
        services.Configure<StorageOptions>(configuration.GetSection(StorageOptions.SectionName));
        services.AddSingleton<IFileStorageService, MinioFileStorageService>();

        // 2. Notifications (WhatsApp)
        services.Configure<WhatsAppOptions>(configuration.GetSection(WhatsAppOptions.SectionName));
        
        services.AddHttpClient<INotificationService, WhatsAppNotificationService>()
            .AddStandardResilienceHandler(options =>
            {
                options.Retry.MaxRetryAttempts = 3;
                options.Retry.BackoffType = DelayBackoffType.Exponential;
                options.Retry.UseJitter = true;
                options.TotalRequestTimeout.Timeout = TimeSpan.FromSeconds(30);

                options.Retry.ShouldHandle = args =>
                    ValueTask.FromResult(args.Outcome.Result?.StatusCode is HttpStatusCode.TooManyRequests);
            });

        return services;
    }
}
