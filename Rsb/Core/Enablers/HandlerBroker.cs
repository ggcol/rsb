﻿using Rsb.Abstractions;
using Rsb.Accessories.Heavy;

namespace Rsb.Core.Enablers;

internal sealed class HandlerBroker<TMessage>(
    IHandleMessage<TMessage> handler,
    IMessagingContext context,
    IHeavyIO heavyIo)
    : BrokerBehavior<TMessage>(context, heavyIo), IHandlerBroker<TMessage>
    where TMessage : IAmAMessage
{
    public async Task Handle(BinaryData binaryData,
        CancellationToken cancellationToken = default)
    {
        var rsbMessage = await GetFrom(binaryData, cancellationToken);

        await handler.Handle(rsbMessage.Message, _context, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task HandleError(Exception ex,
        CancellationToken cancellationToken = default)
    {
        await handler.HandleError(ex, _context, cancellationToken)
            .ConfigureAwait(false);
    }
}