using ASureBus.Abstractions;
using ASureBus.Configurations;
using ASureBus.Configurations.ConfigObjects;
using ASureBus.Core.Caching;
using ASureBus.Core.Messaging;
using ASureBus.Core.Sagas;
using ASureBus.Core.TypesHandling;
using ASureBus.Services.ServiceBus;
using ASureBus.Utils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ASureBus.Core.DI;

public static class MinimalSetup
{
    public static IHostBuilder UseRsb<TSettings>(this IHostBuilder hostBuilder)
        where TSettings : class, IConfigureAzureServiceBus, new()
    {
        hostBuilder.ConfigureServices((hostBuilderContext, _) =>
        {
            var settings = ConfigProvider.LoadSettings<TSettings>(hostBuilderContext.Configuration);

            RsbConfiguration.ServiceBus = new ServiceBusConfig
            {
                ServiceBusConnectionString = settings.ServiceBusConnectionString
            };
        });

        return UseRsb(hostBuilder);
    }

    public static IHostBuilder UseRsb(this IHostBuilder hostBuilder,
        ServiceBusConfig? serviceBusConfig)
    {
        if (serviceBusConfig is null)
            throw new ConfigurationNullException(nameof(ServiceBusConfig));

        RsbConfiguration.ServiceBus = serviceBusConfig;

        return UseRsb(hostBuilder);
    }

    private static IHostBuilder UseRsb(IHostBuilder hostBuilder)
    {
        return hostBuilder
            .ConfigureServices((_, services) =>
            {
                /*
                 * TODO think of this:
                 * what's registered here is broad-wide available in the application...
                 */
                services
                    .AddSingleton<IRsbCache, RsbCache>()
                    .AddSingleton<ITypesLoader, TypesLoader>()
                    .AddSingleton<IAzureServiceBusService, AzureServiceBusService>()
                    .AddSingleton<ISagaBehaviour, SagaBehaviour>()
                    .AddSingleton<IMessagingContext, MessagingContext>()
                    .AddSingleton<IMessageEmitter, MessageEmitter>();

                services.AddHostedService<RsbWorker>();
            });
    }
}