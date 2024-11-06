using ASureBus.Abstractions;
using ASureBus.Accessories.Heavy;
using ASureBus.Core.Entities;
using ASureBus.Services.StorageAccount;

namespace ASureBus.Core.Messaging;

internal abstract class CollectMessage : ICollectMessage
{
    private readonly IHeavyIO? _heavyIo = RsbConfiguration.UseHeavyProperties
        ? new HeavyIO(new AzureDataStorageService(RsbConfiguration.HeavyProps?.DataStorageConnectionString))
        : null;

    public Queue<IAsbMessage> Messages { get; } = new();
    public Guid CorrelationId { get; set; } = Guid.NewGuid();

    //TODO rename
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
        IReadOnlyList<HeavyRef>? heavies = null, DateTimeOffset? scheduledTime = null)
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

    private async Task<IReadOnlyList<HeavyRef>> UnloadHeavies<TMessage>(TMessage message,
        Guid messageId, CancellationToken cancellationToken)
        where TMessage : IAmAMessage
    {
        var heaviesRef = new List<HeavyRef>();

        if (_heavyIo is not null)
        {
            heaviesRef.AddRange(await _heavyIo.Unload(message, messageId, cancellationToken)
                .ConfigureAwait(false));
        }

        return heaviesRef;
    }
}