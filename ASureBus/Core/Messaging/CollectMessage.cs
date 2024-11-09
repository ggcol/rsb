using ASureBus.Abstractions;
using ASureBus.Accessories.Heavy;
using ASureBus.Core.Entities;

namespace ASureBus.Core.Messaging;

internal abstract class CollectMessage : ICollectMessage
{
    public Queue<IAsbMessage> Messages { get; } = new();
    public Guid CorrelationId { get; set; } = Guid.NewGuid();

    protected async Task Enqueue<TMessage>(TMessage message,
        CancellationToken cancellationToken)
        where TMessage : IAmAMessage
    {
        var messageId = Guid.NewGuid();

        var heaviesRef = await UnloadHeavies(message, messageId, cancellationToken)
            .ConfigureAwait(false);

        Messages.Enqueue(ToInternalMessage(message, messageId, heaviesRef));
    }

    protected async Task Enqueue<TMessage>(TMessage message, DateTimeOffset scheduledTime,
        CancellationToken cancellationToken)
        where TMessage : IAmAMessage
    {
        var messageId = Guid.NewGuid();

        var heaviesRef = await UnloadHeavies(message, messageId, cancellationToken)
            .ConfigureAwait(false);

        Messages.Enqueue(ToInternalMessage(message, messageId, heaviesRef, scheduledTime));
    }

    private AsbMessage<TMessage> ToInternalMessage<TMessage>(TMessage message, Guid messageId,
        IReadOnlyList<HeavyReference>? heavies = null, DateTimeOffset? scheduledTime = null)
        where TMessage : IAmAMessage
    {
        return new AsbMessage<TMessage>
        {
            MessageId = messageId,
            MessageName = typeof(TMessage).Name,
            Message = message,
            CorrelationId = CorrelationId,
            Heavies = heavies,
            ScheduledTime = scheduledTime
        };
    }

    private async Task<IReadOnlyList<HeavyReference>> UnloadHeavies<TMessage>(TMessage message,
        Guid messageId, CancellationToken cancellationToken)
        where TMessage : IAmAMessage
    {
        var heaviesRef = new List<HeavyReference>();

        if (HeavyIo.IsHeavyConfigured())
        {
            heaviesRef.AddRange(await HeavyIo.Unload(message, messageId, cancellationToken)
                .ConfigureAwait(false));
        }

        return heaviesRef;
    }
}