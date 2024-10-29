using Rsb.Abstractions;
using Rsb.Accessories.Heavy;
using Rsb.Core.Entities;
using Rsb.Services.StorageAccount;

namespace Rsb.Core.Messaging;

internal abstract class CollectMessage()
    : ICollectMessage
{
    private readonly IHeavyIO? _heavyIo = RsbConfiguration.UseHeavyProperties
        ? new HeavyIO(new AzureDataStorageService(RsbConfiguration.HeavyProps?.DataStorageConnectionString))
        : null;

    public Queue<IRsbMessage> Messages { get; } = new();
    public Guid CorrelationId { get; set; } = Guid.NewGuid();

    //TODO rename
    protected async Task InnerProcessing<TMessage>(TMessage message,
        CancellationToken cancellationToken) where TMessage : IAmAMessage
    {
        var messageId = Guid.NewGuid();

        var heaviesRef = new List<HeavyRef>();

        if (_heavyIo != null)
        {
            heaviesRef.AddRange(
                await _heavyIo
                    .Unload(message, messageId, cancellationToken)
                    .ConfigureAwait(false));
        }

        Messages.Enqueue(new RsbMessage<TMessage>
        {
            MessageId = messageId,
            MessageName = typeof(TMessage).Name,
            Message = message,
            CorrelationId = CorrelationId,
            Heavies = heaviesRef.Count != 0 ? heaviesRef : null
        });
    }
}