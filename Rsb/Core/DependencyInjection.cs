using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Rsb.Configurations;
using Rsb.Services;
using Rsb.Utils;

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
                        hostBuilderContext.Configuration.GetSection(typeof(TSettings).Name));

                services
                    .AddMandatoryServices<TSettings>()
                    .RegisterHandlers();

                services.AddHostedService<RsbWorker>();
            });
    }

    private static IServiceCollection AddMandatoryServices<TSettings>(this IServiceCollection services)
        where TSettings : class, IConfigureAzureServiceBus, new()
    {
        return services
            .AddMemoryCache()
            .AddSingleton<IAzureServiceBusService, AzureServiceBusService<TSettings>>()
            .AddScoped<IMessagingContext, MessagingContext>()
            .AddScoped<IMessageEmitter, MessageEmitter>();
    }
    
    private static IServiceCollection RegisterHandlers(this IServiceCollection services)
    {
        var handlersTypes =
            AssemblySearcher.GetIHandleMessageImplementersTypes(
                Assembly.GetEntryAssembly());

        var messageTypes =
            AssemblySearcher.GetIHandleMessageImplementersMessageTypes(
                handlersTypes);


        foreach (var messageType in messageTypes)
        {
            var handlerType =
                AssemblySearcher
                    .GetIHandleMessageImplementerByMessageType(
                        handlersTypes, messageType);

            var handlerServiceType = typeof(IHandleMessage<>)
                .MakeGenericType(messageType);

            //TODO this may be transient?
            //add handlers as services
            services.AddScoped(handlerServiceType, handlerType);
        }

        return services;
    }
}