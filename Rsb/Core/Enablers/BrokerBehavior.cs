using Rsb.Accessories.Heavy;
using Rsb.Core.Entities;
using Rsb.Core.Messaging;
using Rsb.Utils;

namespace Rsb.Core.Enablers;

internal abstract class BrokerBehavior<TMessage>(
    IMessagingContext context,
    //TODO remove this dependency
    IHeavyIO heavyIo)
    where TMessage : IAmAMessage
{
    public ICollectMessage Collector => (ICollectMessage)_context;
    protected readonly IMessagingContext _context = context;

    protected async Task<RsbMessage<TMessage>?> GetFrom(BinaryData binaryData,
        CancellationToken cancellationToken = default)
    {
        var rsbMessage = await Serializer
            .Deserialize<RsbMessage<TMessage>?>(binaryData.ToStream(), 
                cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        if (RsbConfiguration.UseHeavyProperties &&
            rsbMessage?.Heavies is not null && rsbMessage.Heavies.Any())
        {
            await heavyIo.Load(rsbMessage.Message, rsbMessage.Heavies,
                    rsbMessage.MessageId, cancellationToken)
                .ConfigureAwait(false);
        }

        return rsbMessage;
    }
}