using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Rsb.Configurations;
using Rsb.Services;
using Rsb.Utils;
using Rsb.Workers;

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
                    .AddMemoryCache()
                    .AddSingleton<IAzureServiceBusService,
                        AzureServiceBusService<TSettings>>()
                    .AddTransient<IMessagingContext, MessagingContext>()
                    .AddScoped<IMessageEmitter, MessageEmitter>();

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

                    var brokerServiceType =
                        typeof(IListenerBroker<>).MakeGenericType(messageType);

                    var brokerImplType =
                        typeof(ListenerBroker<>).MakeGenericType(messageType);

                    //TODO this may be transient?
                    //add brokers as services, handlers are then injected
                    services.AddScoped(brokerServiceType, brokerImplType);
                }

                services.AddHostedService<RsbWorker>();
            });
    }
}