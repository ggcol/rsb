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

                //get all queue/sub name (IAmAMessage)
                //handlers
                var listenerTypes = AssemblySearcher.GetListeners();

                foreach (var listenerType in listenerTypes)
                {
                    var messageType = listenerType
                        .GetInterfaces()
                        //TODO multiple IHandleMessageDefinition?
                        .FirstOrDefault(i => i.IsGenericType &&
                                             i.GetGenericTypeDefinition() ==
                                             typeof(IHandleMessage<>))?
                        .GetGenericArguments()
                        .First()!;

                    var listenerServiceType = typeof(IHandleMessage<>)
                        .MakeGenericType(messageType);

                    //add listener
                    //TODO this may be transient?
                    services.AddScoped(listenerServiceType, listenerType);

                    var brokerServiceType =
                        typeof(IListenerBroker<>).MakeGenericType(messageType);

                    var brokerImplType =
                        typeof(ListenerBroker<>).MakeGenericType(messageType);

                    //TODO this may be transient?
                    services.AddScoped(brokerServiceType, brokerImplType);
                }

                services.AddHostedService<RsbWorker>();
            });
    }
}