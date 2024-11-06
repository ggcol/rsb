using ASureBus.Accessories.Heavy;
using ASureBus.Configurations;
using ASureBus.Configurations.ConfigObjects;
using ASureBus.Utils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace ASureBus.Core.DI;

public static class HeavyPropertiesSetup
{
    public static IHostBuilder UseHeavyProps<TSettings>(
        this IHostBuilder hostBuilder)
        where TSettings : class, IConfigureHeavyProperties, new()
    {
        hostBuilder.ConfigureServices((hostBuilderContext, _) =>
        {
            var settings = ConfigProvider.LoadSettings<TSettings>(hostBuilderContext.Configuration);

            AsbConfiguration.HeavyProps = new HeavyPropertiesConfig
            {
                DataStorageConnectionString = settings.DataStorageConnectionString,
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

        AsbConfiguration.HeavyProps = heavyPropsConfig;

        return UseHeavyProps(hostBuilder);
    }

    private static IHostBuilder UseHeavyProps(IHostBuilder hostBuilder)
    {
        return hostBuilder.ConfigureServices((_, services) =>
        {
            services.AddScoped<IHeavyIO, HeavyIO>();
        });
    }
}