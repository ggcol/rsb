using Rsb.Core.Enablers.Entities;

namespace Rsb.Core.Messaging;

internal sealed class MessagingContext : IMessagingContext, ICollectMessage
{
    public Queue<IRsbMessage> Messages { get; } = new();
    public Guid CorrelationId { get; } = Guid.NewGuid();

    private readonly IMessageEmitter _emitter;

    public MessagingContext(IMessageEmitter emitter)
    {
        _emitter = emitter;
    }

    public async Task Send<TCommand>(TCommand message,
        CancellationToken cancellationToken = default)
        where TCommand : IAmACommand
    {
        Messages.Enqueue(new RsbMessage<TCommand>()
        {
            MessageName = typeof(TCommand).Name,
            Message = message,
            CorrelationId = CorrelationId
        });

        await _emitter.FlushAll(this, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task Publish<TEvent>(TEvent message,
        CancellationToken cancellationToken = default)
        where TEvent : IAmAnEvent
    {
        Messages.Enqueue(new RsbMessage<TEvent>()
        {
            MessageName = typeof(TEvent).Name,
            Message = message,
            CorrelationId = CorrelationId
        });

        await _emitter.FlushAll(this, cancellationToken)
            .ConfigureAwait(false);
    }
}