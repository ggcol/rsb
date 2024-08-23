using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Rsb.Configurations;
using Rsb.Core.Caching;
using Rsb.Core.Messaging;
using Rsb.Core.Sagas;
using Rsb.Core.TypesHandling;
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
        /*
         * TODO think of this:
         * what's registered here is broad-wide available in the application...
         */ 
        
        return services
            .AddSingleton<IRsbCache, RsbCache>()
            .AddSingleton<IRsbTypesLoader, RsbTypesLoader>()
            .AddSingleton<IAzureServiceBusService, AzureServiceBusService<TSettings>>()
            //may be singleton
            .AddScoped<ISagaBehaviour, SagaBehaviour>()
            .AddScoped<IMessagingContext, MessagingContext>()
            .AddScoped<IMessageEmitter, MessageEmitter>();
    }
}