using ASureBus.Abstractions;
using ASureBus.Accessories.Heavy;
using ASureBus.Core.Entities;
using ASureBus.Core.Messaging;
using ASureBus.Services.StorageAccount;
using ASureBus.Utils;

namespace ASureBus.Core.Enablers;

internal abstract class BrokerBehavior<TMessage>(
    IMessagingContext context)
    where TMessage : IAmAMessage
{
    private readonly IHeavyIO? _heavyIo = RsbConfiguration.UseHeavyProperties
        ? new HeavyIO(new AzureDataStorageService(RsbConfiguration.HeavyProps?.DataStorageConnectionString))
        : null;

    protected readonly IMessagingContext _context = context;
    
    public ICollectMessage Collector => (ICollectMessage)_context;

    protected async Task<AsbMessage<TMessage>?> GetFrom(BinaryData binaryData,
        CancellationToken cancellationToken = default)
    {
        var rsbMessage = await Serializer
            .Deserialize<AsbMessage<TMessage>?>(binaryData.ToStream(),
                cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        if (_heavyIo is not null)
        {
            await _heavyIo.Load(rsbMessage.Message, rsbMessage.Heavies,
                    rsbMessage.MessageId, cancellationToken)
                .ConfigureAwait(false);
        }

        return rsbMessage;
    }
}