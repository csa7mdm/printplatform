using Microsoft.Extensions.DependencyInjection;
using PrintPlatform.Application.Dispatch;

namespace PrintPlatform.Infrastructure.Dispatch
{
    public static class DispatchServiceCollectionExtensions
    {
        public static IServiceCollection AddDispatchInfrastructure(this IServiceCollection services)
        {
            // If there were any dispatch-specific services, they would be registered here.
            // For example, a concrete implementation of an IDispatchRepository if we were using one.
            return services;
        }
    }
}
