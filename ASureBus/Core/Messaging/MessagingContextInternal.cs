using ASureBus.Abstractions;

namespace ASureBus.Core.Messaging;

internal sealed class MessagingContextInternal : CollectMessage, IMessagingContext
{
    public async Task Send<TCommand>(TCommand message,
        CancellationToken cancellationToken = default)
        where TCommand : IAmACommand
    {
        await Enqueue(message, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task SendAfter<TCommand>(TCommand message, TimeSpan delay,
        CancellationToken cancellationToken = default) where TCommand : IAmACommand
    {
        await SendScheduled(message, DateTimeOffset.UtcNow.Add(delay), cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task SendScheduled<TCommand>(TCommand message, DateTimeOffset scheduledTime,
        CancellationToken cancellationToken = default) where TCommand : IAmACommand
    {
        await Enqueue(message, scheduledTime, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task Publish<TEvent>(TEvent message,
        CancellationToken cancellationToken = default)
        where TEvent : IAmAnEvent
    {
        await Enqueue(message, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task PublishAfter<TEvent>(TEvent message, TimeSpan delay,
        CancellationToken cancellationToken = default) where TEvent : IAmAnEvent
    {
        await PublishScheduled(message, DateTimeOffset.UtcNow.Add(delay), cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task PublishScheduled<TEvent>(TEvent message, DateTimeOffset scheduledTime,
        CancellationToken cancellationToken = default) where TEvent : IAmAnEvent
    {
        await Enqueue(message, scheduledTime, cancellationToken)
            .ConfigureAwait(false);
    }
}