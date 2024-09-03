using System.Text.Json;
using Rsb.Accessories;
using Rsb.Accessories.Heavy;
using Rsb.Core.Enablers.Entities;
using Rsb.Core.Messaging;

namespace Rsb.Core.Enablers;

internal abstract class BrokerBehavior<TMessage>(
    IMessagingContext context,
    IHeavyIO heavyIo)
    where TMessage : IAmAMessage
{
    public ICollectMessage Collector => (ICollectMessage)_context;
    protected readonly IMessagingContext _context = context;

    protected async Task<RsbMessage<TMessage>?> GetFrom(BinaryData binaryData,
        CancellationToken cancellationToken = default)
    {
        var rsbMessage = await Deserialize(binaryData, cancellationToken);

        if (rsbMessage?.Heavies is not null && rsbMessage.Heavies.Any())
        {
            await heavyIo.Load(rsbMessage.Message, rsbMessage.Heavies,
                    rsbMessage.MessageId, cancellationToken)
                .ConfigureAwait(false);
        }

        return rsbMessage;
    }
    
    private static async Task<RsbMessage<TMessage>?> Deserialize(
        BinaryData binaryData, CancellationToken cancellationToken)
    {
        return await JsonSerializer
            .DeserializeAsync<RsbMessage<TMessage>>(binaryData.ToStream(),
                cancellationToken: cancellationToken)
            .ConfigureAwait(false);
    }

}