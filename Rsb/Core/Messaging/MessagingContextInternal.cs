﻿using Rsb.Abstractions;
using Rsb.Accessories.Heavy;

namespace Rsb.Core.Messaging;

internal sealed class MessagingContextInternal(IHeavyIO heavyIo)
    : CollectMessage(heavyIo), IMessagingContext
{
    public async Task Send<TCommand>(TCommand message,
        CancellationToken cancellationToken = default)
        where TCommand : IAmACommand
    {
        await InnerProcessing(message, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task Publish<TEvent>(TEvent message,
        CancellationToken cancellationToken = default)
        where TEvent : IAmAnEvent
    {
        await InnerProcessing(message, cancellationToken)
            .ConfigureAwait(false);
    }
}