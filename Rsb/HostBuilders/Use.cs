using System.Reflection;
using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using MoreLinq.Extensions;
using Rsb.Configurations;
using Rsb.Services;
using Type = System.Type;

namespace Rsb.HostBuilders;

public static class Use
{
    public static IHostBuilder Rsb(this IHostBuilder hostBuilder)
    {
        return hostBuilder
            .ConfigureServices((hostBuilderContext, services) =>
            {
                services
                    .Configure<SuperLocalSettings>(
                        hostBuilderContext.Configuration.GetSection("TODO"));

                services
                    .AddMemoryCache()
                    .AddSingleton<IAzureServiceBusService,
                        AzureServiceBusService<SuperLocalSettings>>();

                //get all queue/sub name (IAmAMessage)
                var listenerTypes = Assembly.GetEntryAssembly()?
                    .GetTypes()
                    .Where(t => t is { IsClass: true, IsAbstract: false })
                    .Where(t =>
                        t.BaseType is not null
                        &&
                        t.BaseType.IsGenericType
                        &&
                        (
                            t.BaseType.GetGenericTypeDefinition() ==
                            typeof(IHandleMessage<>)
                            // || 
                            // t.BaseType.GetGenericTypeDefinition() == typeof(Saga<>)
                        )
                    );

                foreach (var listenerType in listenerTypes)
                {
                    var messageType = listenerType
                        .GetInterfaces()
                        .FirstOrDefault(i => i.IsGenericType &&
                                             i.GetGenericTypeDefinition() ==
                                             typeof(IHandleMessage<>))?
                        .GetGenericArguments()
                        .First()!;

                    var listenerServiceType = typeof(IHandleMessage<>)
                        .MakeGenericType(messageType);

                    //add listener
                    services.AddScoped(listenerServiceType, listenerType);

                    var eggServiceType =
                        typeof(IEgg<>).MakeGenericType(messageType);

                    var eggImplType =
                        typeof(Egg<>).MakeGenericType(messageType);

                    services.AddScoped(eggServiceType, eggImplType);
                }

                services.AddHostedService<MyWorker>();
            });
    }
}

internal class MyWorker : IHostedService
{
    private readonly IHostApplicationLifetime _hostApplicationLifetime;
    private readonly IServiceProvider _serviceProvider;
    private readonly IAzureServiceBusService _azureServiceBusService;

    private readonly IDictionary<Type, ServiceBusProcessor> _processors = new
        Dictionary<Type, ServiceBusProcessor>();

    public MyWorker(
        IHostApplicationLifetime hostApplicationLifetime,
        IServiceProvider serviceProvider,
        IAzureServiceBusService azureServiceBusService)
    {
        _hostApplicationLifetime = hostApplicationLifetime;
        _serviceProvider = serviceProvider;
        _azureServiceBusService = azureServiceBusService;

        var messagesTypes = new List<Type>();

        _serviceProvider
            .GetServices(typeof(IEgg<>))
            .ForEach(service =>
            {
                var type = service
                    .GetType()
                    .GetInterfaces()
                    .FirstOrDefault(i => i.IsGenericType &&
                                         i.GetGenericTypeDefinition() ==
                                         typeof(IEgg<>))?
                    .GetGenericArguments()
                    .First()!;

                messagesTypes.Add(type);
            });

        foreach (var messageType in messagesTypes)
        {
            //TODO subscriptions?
            _processors
                .Add(messageType,
                    _azureServiceBusService.GetProcessor(messageType.Name));
        }
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        foreach (var processor in _processors)
        {
            //IHandleMessage<T> - blblblaHandler<T>
            var egg = _serviceProvider
                .GetServices(typeof(IEgg<>))
                .FirstOrDefault(s => s
                        .GetType()
                        .GetInterfaces()
                        .FirstOrDefault(i => i.IsGenericType
                                             && i.GetGenericTypeDefinition() ==
                                             typeof(IEgg<>))?
                        .GetGenericArguments()
                        .First()! == processor.Key
                ) as IEgg;

            processor.Value.ProcessMessageAsync += async (args) =>
            {
                await egg.EggHandle(args.Message.Body, context);
            };

            processor.Value.ProcessErrorAsync +=
                (args) => egg.HandleErrors(args);
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        _hostApplicationLifetime.StopApplication();
    }
}

public class SuperLocalSettings : IConfigureAzureServiceBus
{
    public string SbConnectionString { get; set; }
        = "";
}

internal interface IEgg
{
    public Task EggHandle(BinaryData binaryData,
        IMessagingContext context);

    //TODO EggHandleError
}

internal interface IEgg<T> : IEgg
{
}

internal sealed class Egg<T> : IEgg<T> where T : IAmAMessage
{
    private IHandleMessage<T> _handler { get; set; }

    public Egg(IHandleMessage<T> handler)
    {
        _handler = handler;
    }

    //TODO messagging context MAY BE dircetly injected in the class
    public async Task EggHandle(BinaryData binaryData,
        IMessagingContext context)
    {
        //TODO check if async is ok
        var message = await Deserialize(binaryData);

        await _handler.Handle(message, context).ConfigureAwait(false);
    }

    private static async Task<T?> Deserialize(BinaryData binaryData)
    {
        return await JsonSerializer
            .DeserializeAsync<T>(binaryData.ToStream())
            .ConfigureAwait(false);
    }
}