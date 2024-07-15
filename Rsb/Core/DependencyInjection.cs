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
    public static IHostBuilder UseRsb(this IHostBuilder hostBuilder)
    {
        return hostBuilder
            .ConfigureServices((hostBuilderContext, services) =>
            {
                services
                    .Configure<SuperLocalSettings>(
                        hostBuilderContext.Configuration.GetSection("TODO"));

                services
                    .AddMemoryCache()
                    .AddSingleton<IAzureServiceBusService, AzureServiceBusService<SuperLocalSettings>>()
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

public class SuperLocalSettings : IConfigureAzureServiceBus
{
    public string SbConnectionString { get; set; }
        = "";
}