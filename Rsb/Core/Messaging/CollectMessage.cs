using Rsb.Accessories.Heavy;
using Rsb.Core.Entities;

namespace Rsb.Core.Messaging;

internal abstract class CollectMessage(IHeavyIO heavyIo)
    : ICollectMessage
{
    public Queue<IRsbMessage> Messages { get; } = new();

    public Guid CorrelationId { get; set; } = Guid.NewGuid();

    //TODO rename
    protected async Task InnerProcessing<TMessage>(TMessage message,
        CancellationToken cancellationToken) where TMessage : IAmAMessage
    {
        var messageId = Guid.NewGuid();
        
        var heaviesRef = await heavyIo
            .Unload(message, messageId, cancellationToken)
            .ConfigureAwait(false);

        var rsbMessage = new RsbMessage<TMessage>
        {
            MessageId = messageId,
            MessageName = typeof(TMessage).Name,
            Message = message,
            CorrelationId = CorrelationId,
            Heavies = heaviesRef.Count != 0 ? heaviesRef : null
        };

        Messages.Enqueue(rsbMessage);
    }
}