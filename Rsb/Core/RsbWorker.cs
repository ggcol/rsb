using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Rsb.Core.Enablers;
using Rsb.Core.Enablers.Entities;
using Rsb.Core.TypesHandling;
using Rsb.Core.TypesHandling.Entities;
using Rsb.Services;

namespace Rsb.Core;

internal sealed class RsbWorker : IHostedService
{
    private readonly IHostApplicationLifetime _hostApplicationLifetime;
    private readonly IServiceProvider _serviceProvider;
    private readonly IMessageEmitter _messageEmitter;

    private readonly IDictionary<ListenerType, ServiceBusProcessor>
        _processors = new Dictionary<ListenerType, ServiceBusProcessor>();

    private readonly IMemoryCache _memoryCache;

    public RsbWorker(
        IHostApplicationLifetime hostApplicationLifetime,
        IServiceProvider serviceProvider,
        IAzureServiceBusService azureServiceBusService,
        IMessageEmitter messageEmitter,
        IRsbTypesLoader rsbTypesLoader, IMemoryCache memoryCache)
    {
        _hostApplicationLifetime = hostApplicationLifetime;
        _serviceProvider = serviceProvider;
        _messageEmitter = messageEmitter;
        _memoryCache = memoryCache;

        foreach (var handler in rsbTypesLoader.Handlers)
        {
            var processor = GetProcessor(
                    azureServiceBusService,
                    handler,
                    hostApplicationLifetime.ApplicationStopping)
                .ConfigureAwait(false).GetAwaiter().GetResult();

            processor.ProcessMessageAsync += async (args)
                => await ProcessMessage(handler, args);

            processor.ProcessErrorAsync += async (args)
                => await ProcessError(handler, args);

            _processors.Add(handler, processor);
        }

        foreach (var saga in rsbTypesLoader.Sagas)
        {
            foreach (var listener in saga.Listeners)
            {
                var processor = GetProcessor(
                        azureServiceBusService,
                        listener,
                        hostApplicationLifetime.ApplicationStopping)
                    .ConfigureAwait(false).GetAwaiter().GetResult();

                processor.ProcessMessageAsync += async (args) =>
                    await ProcessMessage(saga, listener, args);

                processor.ProcessErrorAsync += async (args)
                    => await ProcessError(saga, listener, args);

                _processors.Add(listener, processor);
            }
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
        IAzureServiceBusService azureServiceBusService, ListenerType handler,
        CancellationToken cancellationToken)
    {
        switch (handler.MessageType.IsCommand)
        {
            case true:
            {
                var queue = await azureServiceBusService
                    .ConfigureQueue(handler.MessageType.Type.Name)
                    .ConfigureAwait(false);
                return azureServiceBusService.GetProcessor(queue);
            }
            case false:
            {
                var topicConfig = await azureServiceBusService
                    .ConfigureTopicForReceiver(handler.MessageType.Type)
                    .ConfigureAwait(false);
                return azureServiceBusService.GetProcessor(topicConfig.Name,
                    topicConfig.SubscriptionName);
            }
        }
    }

    private async Task ProcessError(HandlerType handlerType,
        ProcessErrorEventArgs args)
    {
        var broker = GetBroker(_serviceProvider, handlerType);

        await broker.HandleError(args.Exception, args.CancellationToken)
            .ConfigureAwait(false);
    }

    private async Task ProcessMessage(HandlerType handlerType,
        ProcessMessageEventArgs args)
    {
        var broker = GetBroker(_serviceProvider, handlerType);

        await broker
            .Handle(args.Message.Body, args.CancellationToken)
            .ConfigureAwait(false);

        await args
            .CompleteMessageAsync(args.Message, args.CancellationToken)
            .ConfigureAwait(false);

        await _messageEmitter.FlushAll(broker.Collector, args.CancellationToken)
            .ConfigureAwait(false);
    }

    private async Task ProcessError(SagaType sagaType,
        ListenerType listenerType,
        ProcessErrorEventArgs args)
    {
        //TODO!
        throw new NotImplementedException();
    }

    private async Task ProcessMessage(SagaType sagaType,
        ListenerType listenerType,
        ProcessMessageEventArgs args)
    {
        var corrId = await JsonSerializer
            .DeserializeAsync<DeserializeCorrelationId>(
                args.Message.Body.ToStream(),
                cancellationToken: args.CancellationToken)
            .ConfigureAwait(false);

        var correlationId = corrId.CorrelationId;

        var broker = GetBroker(_serviceProvider, sagaType, listenerType,
            correlationId);

        await broker.Handle(args.Message.Body, args.CancellationToken)
            .ConfigureAwait(false);

        await args.CompleteMessageAsync(args.Message, args.CancellationToken)
            .ConfigureAwait(false);

        await _messageEmitter.FlushAll(broker.Collector, args.CancellationToken)
            .ConfigureAwait(false);
    }

    private static IListenerBroker GetBroker(IServiceProvider serviceProvider,
        HandlerType handlerType)
    {
        var implListener = ActivatorUtilities.CreateInstance(
            serviceProvider, handlerType.Type);

        var implType = typeof(HandlerBroker<>)
            .MakeGenericType(handlerType.MessageType.Type);

        return (IListenerBroker)ActivatorUtilities.CreateInstance(
            serviceProvider, implType, implListener);
    }

    private ISagaBroker GetBroker(IServiceProvider serviceProvider,
        SagaType sagaType, ListenerType listenerType, Guid correlationId)
    {
        object? implSaga;
        
        if (_memoryCache.TryGetValue(correlationId, out var saga))
        {
            implSaga = saga;
        }
        else
        {
            implSaga = ActivatorUtilities.CreateInstance(serviceProvider,
                    sagaType.Type);

            _memoryCache.Set(correlationId, implSaga);
        }

        var brokerImplType = typeof(SagaBroker<,>)
            .MakeGenericType(sagaType.SagaDataType,
                listenerType.MessageType.Type);

        return (ISagaBroker)ActivatorUtilities.CreateInstance(serviceProvider,
                brokerImplType, implSaga);
    }
}