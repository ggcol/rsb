using ASureBus.Configurations;
using ASureBus.Configurations.ConfigObjects;
using ASureBus.Utils;
using Microsoft.Extensions.Hosting;

namespace ASureBus.Core.DI;

public static class AsbCacheSetup
{
    public static IHostBuilder ConfigureAsbCache<TSettings>(
        this IHostBuilder hostBuilder)
        where TSettings : class, IConfigureRsbCache, new()
    {
        return hostBuilder.ConfigureServices((hostBuilderContext, _) =>
        {
            var settings = ConfigProvider.LoadSettings<TSettings>(hostBuilderContext.Configuration);

            RsbConfiguration.Cache = new RsbCacheConfig
            {
                Expiration = settings.Expiration,
                TopicConfigPrefix = settings.TopicConfigPrefix,
                ServiceBusSenderCachePrefix = settings.ServiceBusSenderCachePrefix
            };
        });
    }

    public static IHostBuilder ConfigureAsbCache(this IHostBuilder hostBuilder,
        RsbCacheConfig rsbCacheConfig)
    {
        if (rsbCacheConfig is null)
            throw new ConfigurationNullException(nameof(rsbCacheConfig));

        RsbConfiguration.Cache = rsbCacheConfig;

        return hostBuilder;
    }
}