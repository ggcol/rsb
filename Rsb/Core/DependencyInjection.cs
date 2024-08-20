using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Rsb.Configurations;
using Rsb.Services;

namespace Rsb.Core;

//TODO rename
public static class DependencyInjection
{
    public static IHostBuilder UseRsb<TSettings>(this IHostBuilder hostBuilder)
        where TSettings : class, IConfigureAzureServiceBus, new()
    {
        return hostBuilder
            .ConfigureServices((hostBuilderContext, services) =>
            {
                services
                    .Configure<TSettings>(
                        hostBuilderContext.Configuration.GetSection(
                            typeof(TSettings).Name));

                services
                    .AddMandatoryServices<TSettings>()
                    ;

                services.AddHostedService<RsbWorker>();
            });
    }

    private static IServiceCollection AddMandatoryServices<TSettings>(
        this IServiceCollection services)
        where TSettings : class, IConfigureAzureServiceBus, new()
    {
        return services
            .AddMemoryCache()
            .AddSingleton<IRsbCache, RsbCache>()
            .AddSingleton<IAzureServiceBusService, AzureServiceBusService<TSettings>>()
            .AddScoped<IMessagingContext, MessagingContext>()
            .AddScoped<IMessageEmitter, MessageEmitter>();
    }
}