using ASureBus.Abstractions;
using ASureBus.Abstractions.Options.Messaging;
using ASureBus.Accessories.Heavy;
using ASureBus.Core.Entities;

namespace ASureBus.Core.Messaging;

internal abstract class CollectMessage : ICollectMessage
{
    public Queue<IAsbMessage> Messages { get; } = new();
    public Guid CorrelationId { get; set; } = Guid.NewGuid();

    protected async Task Enqueue<TMessage>(TMessage message, EmitOptions? options, 
        CancellationToken cancellationToken)
        where TMessage : IAmAMessage
    {
        var messageId = Guid.NewGuid();

        var heaviesRef = await UnloadHeavies(message, messageId, cancellationToken)
            .ConfigureAwait(false);

        Messages.Enqueue(ToInternalMessage(message, messageId, options, heaviesRef));
    }
    
    protected async Task Enqueue<TMessage>(TMessage message, DateTimeOffset scheduledTime,
        EmitOptions? options, CancellationToken cancellationToken)
        where TMessage : IAmAMessage
    {
        var messageId = Guid.NewGuid();

        var heaviesRef = await UnloadHeavies(message, messageId, cancellationToken)
            .ConfigureAwait(false);

        Messages.Enqueue(ToInternalMessage(message, messageId, options, heaviesRef, scheduledTime));
    }

    private AsbMessage<TMessage> ToInternalMessage<TMessage>(TMessage message, Guid messageId,
        EmitOptions? options = null, IReadOnlyList<HeavyReference>? heavies = null, 
        DateTimeOffset? scheduledTime = null)
        where TMessage : IAmAMessage
    {
        var messageName = typeof(TMessage).Name;
        var asbMessage = new AsbMessage<TMessage>
        {
            MessageId = messageId,
            MessageName = messageName,
            Destination = options is null || string.IsNullOrWhiteSpace(options.Destination)
                ? messageName
                : options.Destination,
            Message = message,
            CorrelationId = CorrelationId,
        };

        if (heavies is not null && heavies.Count > 0)
        {
            asbMessage.Heavies = heavies;
        }

        if (scheduledTime is not null)
        {
            asbMessage.ScheduledTime = scheduledTime;
        }

        return asbMessage;
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