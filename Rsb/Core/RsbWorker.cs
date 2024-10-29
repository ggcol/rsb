using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Rsb.Core.Caching;
using Rsb.Core.Enablers;
using Rsb.Core.Entities;
using Rsb.Core.Messaging;
using Rsb.Core.Sagas;
using Rsb.Core.TypesHandling;
using Rsb.Core.TypesHandling.Entities;
using Rsb.Services.ServiceBus;
using Rsb.Utils;

namespace Rsb.Core;

internal sealed class RsbWorker : IHostedService
{
    private readonly IHostApplicationLifetime _hostApplicationLifetime;
    private readonly IServiceProvider _serviceProvider;
    private readonly IMessageEmitter _messageEmitter;
    private readonly IRsbCache _cache;
    private readonly ISagaBehaviour _sagaBehaviour;
    private readonly ISagaIO _sagaIo;

    private readonly IDictionary<ListenerType, ServiceBusProcessor>
        _processors = new Dictionary<ListenerType, ServiceBusProcessor>();

    public RsbWorker(
        IHostApplicationLifetime hostApplicationLifetime,
        IServiceProvider serviceProvider,
        IAzureServiceBusService azureServiceBusService,
        IMessageEmitter messageEmitter,
        IRsbTypesLoader rsbTypesLoader,
        IRsbCache cache,
        ISagaBehaviour sagaBehaviour,
        ISagaIO sagaIo)
    {
        _hostApplicationLifetime = hostApplicationLifetime;
        _serviceProvider = serviceProvider;
        _messageEmitter = messageEmitter;
        _cache = cache;
        _sagaBehaviour = sagaBehaviour;
        _sagaIo = sagaIo;

        foreach (var handler in rsbTypesLoader.Handlers)
        {
            var processor = azureServiceBusService.GetProcessor(
                    handler, hostApplicationLifetime.ApplicationStopping)
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
                var processor = azureServiceBusService
                    .GetProcessor(
                        listener, hostApplicationLifetime.ApplicationStopping)
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
        var correlationId = await GetCorrelationId(args);
        
        var broker = GetBroker(_serviceProvider, handlerType, correlationId);

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
        SagaHandlerType listenerType, ProcessMessageEventArgs args)
    {
        var correlationId = await GetCorrelationId(args);

        var implSaga =
            await GetSagaImplementation(_serviceProvider, sagaType,
                    listenerType, correlationId)
                .ConfigureAwait(false);

        var broker = GetBroker(_serviceProvider, sagaType, implSaga,
            listenerType,
            correlationId);

        await broker.Handle(args.Message.Body, args.CancellationToken)
            .ConfigureAwait(false);

        await args.CompleteMessageAsync(args.Message, args.CancellationToken)
            .ConfigureAwait(false);

        if (RsbConfiguration.OffloadSagas)
        {
            await _sagaIo.Unload(implSaga, correlationId, sagaType).ConfigureAwait(false);
        }
        else
        {
            _cache.Set(correlationId, implSaga);
        }
        
        await _messageEmitter.FlushAll(broker.Collector, args.CancellationToken)
            .ConfigureAwait(false);
    }

    private static async Task<Guid> GetCorrelationId(
        ProcessMessageEventArgs args)
    {
        var corrId = await Serializer
            .Deserialize<DeserializeCorrelationId>(
                args.Message.Body.ToStream(),
                cancellationToken: args.CancellationToken)
            .ConfigureAwait(false);

        return corrId.CorrelationId;
    }

    private IHandlerBroker GetBroker(IServiceProvider serviceProvider,
        HandlerType handlerType, Guid? correlationId = null)
    {
        var implListener = ActivatorUtilities.CreateInstance(
            serviceProvider, handlerType.Type);

        var brokerImplType = typeof(HandlerBroker<>)
            .MakeGenericType(handlerType.MessageType.Type);

        var context = new MessagingContextInternal();
        
        if (correlationId is not null)
        {
            context.CorrelationId = correlationId.Value;
        };

        return (IHandlerBroker)ActivatorUtilities.CreateInstance(
            serviceProvider, brokerImplType, implListener, context);
    }

    private ISagaBroker GetBroker(IServiceProvider serviceProvider,
        SagaType sagaType, object? implSaga, ListenerType listenerType,
        Guid correlationId)
    {
        var brokerImplType = typeof(SagaBroker<,>).MakeGenericType(
            sagaType.SagaDataType, listenerType.MessageType.Type);

        var context = new MessagingContextInternal()
        {
            CorrelationId = correlationId
        };

        return (ISagaBroker)ActivatorUtilities.CreateInstance(serviceProvider,
            brokerImplType, implSaga, context);
    }

    private async Task<object?> GetSagaImplementation(
        IServiceProvider serviceProvider, SagaType sagaType,
        SagaHandlerType listenerType, Guid correlationId)
    {
        if (!listenerType.IsInitMessage)
        {
            if (RsbConfiguration.OffloadSagas)
            {
                return await _sagaIo.Load(correlationId, sagaType)
                    .ConfigureAwait(false);
            }

            if (_cache.TryGetValue(correlationId, out var saga))
            {
                return saga;
            }
        }

        var implSaga = ActivatorUtilities.CreateInstance(serviceProvider,
            sagaType.Type);

        _sagaBehaviour.SetCorrelationId(sagaType, correlationId, implSaga);

        _sagaBehaviour.HandleCompletion(sagaType, correlationId, implSaga);

        return _cache.Set(correlationId, implSaga);
    }
}