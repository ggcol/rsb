﻿using ASureBus.Abstractions;
using ASureBus.Accessories.Heavy;
using ASureBus.Core.Entities;
using ASureBus.Core.Messaging;
using ASureBus.Utils;

namespace ASureBus.Core.Enablers;

internal abstract class BrokerBehavior<TMessage>(
    IMessagingContext context)
    where TMessage : IAmAMessage
{
    protected readonly IMessagingContext _context = context;
    
    public ICollectMessage Collector => (ICollectMessage)_context;

    protected async Task<AsbMessage<TMessage>?> GetFrom(BinaryData binaryData,
        CancellationToken cancellationToken = default)
    {
        var asbMessage = await Serializer
            .Deserialize<AsbMessage<TMessage>?>(binaryData.ToStream(),
                cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        if (HeavyIo.IsHeavyConfigured())
        {
            await HeavyIo.Load(asbMessage.Message, asbMessage.Heavies,
                    asbMessage.MessageId, cancellationToken)
                .ConfigureAwait(false);
        }

        return asbMessage;
    }
}