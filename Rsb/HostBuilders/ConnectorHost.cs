using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MoreLinq.Extensions;
using Rsb.Configurations;
using Rsb.Services;
using Rsb.Workers;

namespace Rsb.HostBuilders;

public static class ConnectorHost
{
    public static IHostBuilder Create<TSettings>()
        where TSettings : class, IConfigureAzureServiceBus, new()
    {
        return new HostBuilder()
            .ConfigureServices((hostBuilderContext, services) =>
            {
                //TODO
                services.Configure<TSettings>(
                    hostBuilderContext.Configuration.GetSection("TODO"));

                services.AddMemoryCache();
                
                services
                    .AddScoped<IMessagingContext, MessagingContext>()
                    .AddSingleton<IAzureServiceBusService, AzureServiceBusService<TSettings>>()
                    .AddSingleton<IMessageEmitter, MessageEmitter>();

                Assembly.GetEntryAssembly()?
                    .GetTypes()
                    .Where(t => t is { IsClass: true, IsAbstract: false })
                    .Where(t =>
                        t.GetInterfaces()
                            .Any(i =>
                                i.IsGenericType &&
                                i.GetGenericTypeDefinition() ==
                                typeof(IHandleMessage<>)))
                    .ToArray()
                    .ForEach(handlerType =>
                    {
                        var messageType = handlerType
                            .GetInterfaces()
                            .FirstOrDefault(i => i.IsGenericType &&
                                                 i.GetGenericTypeDefinition() ==
                                                 typeof(IHandleMessage<>))?
                            .GetGenericArguments()
                            .First()!;

                        var serviceType = typeof(IHandleMessage<>)
                            .MakeGenericType(messageType);

                        //add handler
                        services.AddScoped(serviceType, handlerType);

                        /*
                         * add hosted service
                         * each handler will be injected into a worker
                         */
                        var engineImplType = typeof(HandlerWorker<>)
                            .MakeGenericType(messageType);

                        services.AddSingleton(typeof(IHostedService),
                            engineImplType);
                    });
            });
    }
}