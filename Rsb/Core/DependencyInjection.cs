using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Rsb.Accessories.Heavy;
using Rsb.Configurations;
using Rsb.Configurations.ConfigObjects;
using Rsb.Core.Caching;
using Rsb.Core.Messaging;
using Rsb.Core.Sagas;
using Rsb.Core.TypesHandling;
using Rsb.Services.ServiceBus;
using Rsb.Services.StorageAccount;

namespace Rsb.Core;

public static class DependencyInjection
{
    public static IHostBuilder UseRsb<TSettings>(this IHostBuilder hostBuilder)
        where TSettings : class, IConfigureAzureServiceBus, new()
    {
        hostBuilder.ConfigureServices((hostBuilderContext, _) =>
        {
            var settings = ConfigProvider.LoadSettings<TSettings>(
                hostBuilderContext.Configuration);

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
                    .AddSingleton<IRsbTypesLoader, RsbTypesLoader>()
                    .AddSingleton<IAzureServiceBusService,
                        AzureServiceBusService>()
                    //may be singleton
                    .AddScoped<ISagaBehaviour, SagaBehaviour>()
                    .AddScoped<IMessagingContext, MessagingContext>()
                    .AddScoped<IMessageEmitter, MessageEmitter>()
                    .AddScoped<ISagaIO, SagaIO>();

                //TODO remove this dependency
                services.AddScoped<IHeavyIO, UnusedHeavyIO>();

                services.AddHostedService<RsbWorker>();
            });
    }

    public static IHostBuilder UseHeavyProps<TSettings>(
        this IHostBuilder hostBuilder)
        where TSettings : class, IConfigureHeavyProperties, new()
    {
        hostBuilder.ConfigureServices((hostBuilderContext, _) =>
        {
            var settings =
                ConfigProvider.LoadSettings<TSettings>(hostBuilderContext
                    .Configuration);

            RsbConfiguration.HeavyProps = new HeavyPropertiesConfig
            {
                DataStorageConnectionString =
                    settings.DataStorageConnectionString,
                DataStorageContainer = settings.DataStorageContainer
            };
        });

        return UseHeavyProps(hostBuilder);
    }

    public static IHostBuilder UseHeavyProps(
        this IHostBuilder hostBuilder, HeavyPropertiesConfig? heavyPropsConfig)
    {
        if (heavyPropsConfig is null)
            throw new ConfigurationNullException(nameof(HeavyPropertiesConfig));

        RsbConfiguration.HeavyProps = heavyPropsConfig;

        return UseHeavyProps(hostBuilder);
    }

    private static IHostBuilder UseHeavyProps(IHostBuilder hostBuilder)
    {
        return hostBuilder.ConfigureServices((_, services) =>
        {
            //TODO remove this, check UseRsb()
            services
                .Remove(new ServiceDescriptor(typeof(IHeavyIO),
                    typeof(UnusedHeavyIO), ServiceLifetime.Scoped));

            services
                .AddScoped<HeavyPropsStorageService>()
                .AddScoped<IHeavyIO, HeavyIO>();
        });
    }

    public static IHostBuilder ConfigureRsbCache<TSettings>(
        this IHostBuilder hostBuilder)
        where TSettings : class, IConfigureRsbCache, new()
    {
        return hostBuilder.ConfigureServices((hostBuilderContext, _) =>
        {
            var settings =
                ConfigProvider.LoadSettings<TSettings>(hostBuilderContext
                    .Configuration);

            RsbConfiguration.Cache = new RsbCacheConfig
            {
                Expiration = settings.Expiration,
                TopicConfigPrefix = settings.TopicConfigPrefix,
                ServiceBusSenderCachePrefix =
                    settings.ServiceBusSenderCachePrefix
            };
        });
    }

    public static IHostBuilder ConfigureRsbCache(this IHostBuilder hostBuilder,
        RsbCacheConfig rsbCacheConfig)
    {
        if (rsbCacheConfig is null)
            throw new ConfigurationNullException(nameof(rsbCacheConfig));

        RsbConfiguration.Cache = rsbCacheConfig;

        return hostBuilder;
    }

    public static IHostBuilder ConfigureSagaPersistence<TSettings>(
        this IHostBuilder hostBuilder)
        where TSettings : class, IConfigureSagaPersistence, new()
    {
        hostBuilder.ConfigureServices((hostBuilderContext, _) =>
        {
            var settings = ConfigProvider
                .LoadSettings<TSettings>(hostBuilderContext.Configuration);

            RsbConfiguration.SagaPersistence = new SagaPersistenceConfig()
            {
                DataStorageConnectionString =
                    settings.DataStorageConnectionString,
                DataStorageContainer = settings.DataStorageContainer
            };
        });

        return ConfigureSagaPersistence(hostBuilder);
    }

    public static IHostBuilder ConfigureSagaPersistence(
        this IHostBuilder hostBuilder,
        SagaPersistenceConfig sagaPersistenceConfig)
    {
        if (sagaPersistenceConfig is null)
            throw new ConfigurationNullException(nameof(sagaPersistenceConfig));

        RsbConfiguration.SagaPersistence = sagaPersistenceConfig;

        return ConfigureSagaPersistence(hostBuilder);
    }

    private static IHostBuilder ConfigureSagaPersistence(
        IHostBuilder hostBuilder)
    {
        return hostBuilder.ConfigureServices((_, services) =>
        {
            services.AddScoped<SagaPersistenceStorageService>();
        });
    }
}