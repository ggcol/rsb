using Asb.Abstractions;

namespace Asb.Core.Messaging;

internal sealed class MessagingContextInternal : CollectMessage, IMessagingContext
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