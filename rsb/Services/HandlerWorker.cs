#nullable enable
using System.Text.Json;
using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Hosting;
using rsb.Configurations;

namespace rsb.Services;

internal sealed class HandlerWorker<TMessage> : IHostedService
    where TMessage : IAmAMessage
{
    private IHandleMessage<TMessage> _messageHandler;
    private readonly IMessagingContext _messagingContext;
    private readonly IMessageEmitter _messageEmitter;

    private ServiceBusProcessor _sbProcessor { get; set; }

    private event Func<TMessage, IMessagingContext, Task> _processMessage;
    private event Func<Exception, IMessagingContext, Task> _processError;

    public HandlerWorker(
        IAzureServiceBusService serviceBusService,
        IHandleMessage<TMessage> messageHandler,
        IMessagingContext messagingContext,
        IMessageEmitter messageEmitter)
    {
        ConfigureWorker(serviceBusService)
            .ConfigureAwait(false)
            .GetAwaiter()
            .GetResult();

        _messageHandler = messageHandler;
        _messagingContext = messagingContext;
        _messageEmitter = messageEmitter;

        _sbProcessor.ProcessMessageAsync += ProcessMessage;
        _sbProcessor.ProcessErrorAsync += ProcessErrors;

        _processMessage += _messageHandler.Handle;
        _processError += _messageHandler.HandleErrors;
    }

    ~HandlerWorker()
    {
        _sbProcessor.ProcessMessageAsync -= ProcessMessage;
        _sbProcessor.ProcessErrorAsync -= ProcessErrors;

        _processMessage -= _messageHandler.Handle;
        _processError -= _messageHandler.HandleErrors;
    }

    private async Task ConfigureWorker(
        IAzureServiceBusService serviceBusService)
    {
        var messageType = typeof(TMessage);
        var interfaces = messageType.GetInterfaces();

        if (interfaces.Any(i => i == typeof(IAmACommand)))
        {
            var queue = await serviceBusService
                .ConfigureQueue(messageType.Name)
                .ConfigureAwait(false);

            _sbProcessor = serviceBusService.GetProcessor(queue);
            return;
        }

        // ReSharper disable once InvertIf
        if (interfaces.Any(i => i == typeof(IAmAnEvent)))
        {
            var topic = await serviceBusService
                .ConfigureTopicForReceiver(messageType)
                .ConfigureAwait(false);

            _sbProcessor = serviceBusService.GetProcessor(topic.Name,
                    topic.SubscriptionName);
            return;
        }

        //TODO custom ex?
        //message must be either a Command or an Event
        throw new InvalidOperationException();
    }

    private async Task ProcessMessage(ProcessMessageEventArgs args)
    {
        var body = args.Message.Body.ToString();
        var message = JsonSerializer.Deserialize<TMessage>(body);

        await _processMessage
            .Invoke(message ?? throw new NullReferenceException(),
                _messagingContext)
            .ConfigureAwait(false);

        await args.CompleteMessageAsync(args.Message).ConfigureAwait(false);

        await _messageEmitter.FlushAll((ICollectMessage)_messagingContext,
                args.CancellationToken)
            .ConfigureAwait(false);
    }

    private async Task ProcessErrors(ProcessErrorEventArgs args)
    {
        await _processError
            .Invoke(args.Exception, _messagingContext)
            .ConfigureAwait(false);
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        await _sbProcessor.StartProcessingAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await _sbProcessor.StopProcessingAsync(cancellationToken)
            .ConfigureAwait(false);
        await _sbProcessor.DisposeAsync().ConfigureAwait(false);
    }
}