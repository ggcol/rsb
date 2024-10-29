using Microsoft.Extensions.DependencyInjection;
using Rsb.Abstractions;
using Rsb.Accessories.Heavy;
using Rsb.Core.Entities;
using Rsb.Core.Messaging;
using Rsb.Services.StorageAccount;
using Rsb.Utils;

namespace Rsb.Core.Enablers;

internal abstract class BrokerBehavior<TMessage>(
    IMessagingContext context,
    IServiceProvider serviceProvider)
    where TMessage : IAmAMessage
{
    private readonly IHeavyIO? _heavyIo = RsbConfiguration.UseHeavyProperties
        ? new HeavyIO(serviceProvider.GetRequiredService<IAzureDataStorageService>())
        : null;

    protected readonly IMessagingContext _context = context;
    
    public ICollectMessage Collector => (ICollectMessage)_context;

    protected async Task<RsbMessage<TMessage>?> GetFrom(BinaryData binaryData,
        CancellationToken cancellationToken = default)
    {
        var rsbMessage = await Serializer
            .Deserialize<RsbMessage<TMessage>?>(binaryData.ToStream(),
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