using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Rsb.Enablers;
using Rsb.Services;

namespace Rsb.Core;

internal class RsbWorker : IHostedService
{
    private readonly IHostApplicationLifetime _hostApplicationLifetime;
    private readonly IServiceProvider _serviceProvider;
    private readonly IMessageEmitter _messageEmitter;
    private readonly IRsbCache _rsbCache;

    private readonly IDictionary<ListenerType, ServiceBusProcessor>
        _processors = new Dictionary<ListenerType, ServiceBusProcessor>();

    public RsbWorker(
        IHostApplicationLifetime hostApplicationLifetime,
        IServiceProvider serviceProvider,
        IAzureServiceBusService azureServiceBusService,
        IMessageEmitter messageEmitter, IRsbCache rsbCache)
    {
        _hostApplicationLifetime = hostApplicationLifetime;
        _serviceProvider = serviceProvider;
        _messageEmitter = messageEmitter;
        _rsbCache = rsbCache;

        foreach (var listener in _rsbCache.Listeners)
        {
            var processor = GetProcessor(azureServiceBusService, listener,
                    hostApplicationLifetime.ApplicationStopping)
                .ConfigureAwait(false).GetAwaiter().GetResult();

            processor.ProcessMessageAsync += async (args)
                => await ProcessMessage(listener, args);

            processor.ProcessErrorAsync += async (args)
                => await ProcessError(listener, args);

            _processors.Add(listener, processor);
        }
    }
    
    public async Task StartAsync(CancellationToken cancellationToken)
    {
        foreach (var processorKvp in _processors)
        {
            await processorKvp.Value.StartProcessingAsync(cancellationToken)
                .ConfigureAwait(false);
        }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        foreach (var processorKvp in _processors)
        {
            await processorKvp.Value.StopProcessingAsync(cancellationToken)
                .ConfigureAwait(false);
            await processorKvp.Value.DisposeAsync().ConfigureAwait(false);
        }

        _hostApplicationLifetime.StopApplication();
    }

    private static async Task<ServiceBusProcessor> GetProcessor(
        IAzureServiceBusService azureServiceBusService, ListenerType listener,
        CancellationToken cancellationToken)
    {
        switch (listener.MessageType.IsCommand)
        {
            case true:
            {
                var queue = await azureServiceBusService
                    .ConfigureQueue(listener.MessageType.Type.Name)
                    .ConfigureAwait(false);
                return azureServiceBusService.GetProcessor(queue);
            }
            case false:
            {
                var topicConfig = await azureServiceBusService
                    .ConfigureTopicForReceiver(listener.MessageType.Type)
                    .ConfigureAwait(false);
                return azureServiceBusService.GetProcessor(topicConfig.Name,
                    topicConfig.SubscriptionName);
            }
        }
    }

    private async Task ProcessError(ListenerType listenerType,
        ProcessErrorEventArgs args)
    {
        var broker = GetBroker(_serviceProvider, listenerType);

        await broker.HandleError(args.Exception, args.CancellationToken)
            .ConfigureAwait(false);
    }

    private async Task ProcessMessage(ListenerType listenerType,
        ProcessMessageEventArgs args)
    {
        var broker = GetBroker(_serviceProvider, listenerType);

        await broker
            .Handle(args.Message.Body, args.CancellationToken)
            .ConfigureAwait(false);

        await args
            .CompleteMessageAsync(args.Message, args.CancellationToken)
            .ConfigureAwait(false);

        await _messageEmitter.FlushAll(broker.Collector,
            args.CancellationToken).ConfigureAwait(false);
    }

    private static IListenerBroker GetBroker(IServiceProvider serviceProvider,
        ListenerType listenerType)
    {
        var implListener = ActivatorUtilities.CreateInstance(
            serviceProvider, listenerType.Type);

        var implType = typeof(ListenerBroker<>)
            .MakeGenericType(listenerType.MessageType.Type);
        
        return (IListenerBroker)ActivatorUtilities.CreateInstance(
            serviceProvider, implType, implListener);
    }
}