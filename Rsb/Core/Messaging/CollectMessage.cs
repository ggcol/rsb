using Rsb.Accessories.Heavy;
using Rsb.Configurations;
using Rsb.Core.Entities;

namespace Rsb.Core.Messaging;

internal abstract class CollectMessage(IHeavyIO heavyIo)
    : ICollectMessage
{
    private Guid _correlationId = Guid.NewGuid();
    private bool _isUserDefinedCorrelationId;

    public Queue<IRsbMessage> Messages { get; } = new();

    public Guid CorrelationId
    {
        get => _correlationId;
        set
        {
            _correlationId = value;
            _isUserDefinedCorrelationId = true;
        }
    }

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