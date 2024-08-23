using Rsb.Core.Enablers.Entities;

namespace Rsb.Core.Messaging;

internal sealed class MessagingContextInternal : IMessagingContext, ICollectMessage
{
    public Queue<IRsbMessage> Messages { get; } = new();
    public Guid CorrelationId { get; init; } = Guid.NewGuid();
    
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
    }
}