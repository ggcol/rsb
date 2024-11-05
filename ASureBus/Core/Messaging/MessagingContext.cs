using ASureBus.Abstractions;

namespace ASureBus.Core.Messaging;

internal sealed class MessagingContext(IMessageEmitter emitter)
    : CollectMessage(), IMessagingContext
{
    public Task Send<TCommand>(TCommand message, CancellationToken cancellationToken = default)
        where TCommand : IAmACommand
    {
        return EnqueueAndFlush(message, cancellationToken);
    }

    public Task SendAfter<TCommand>(TCommand message, TimeSpan delay,
        CancellationToken cancellationToken = default)
        where TCommand : IAmACommand
    {
        return SendScheduled(message, DateTimeOffset.UtcNow.Add(delay), cancellationToken);
    }

    public Task SendScheduled<TCommand>(TCommand message, DateTimeOffset scheduledTime,
        CancellationToken cancellationToken = default)
        where TCommand : IAmACommand
    {
        return EnqueueAndFlush(message, scheduledTime, cancellationToken);
    }

    public Task Publish<TEvent>(TEvent message, CancellationToken cancellationToken = default)
        where TEvent : IAmAnEvent
    {
        return EnqueueAndFlush(message, cancellationToken);
    }

    public Task PublishAfter<TEvent>(TEvent message, TimeSpan delay,
        CancellationToken cancellationToken = default)
        where TEvent : IAmAnEvent
    {
        return PublishScheduled(message, DateTimeOffset.UtcNow.Add(delay), cancellationToken);
    }

    public Task PublishScheduled<TEvent>(TEvent message, DateTimeOffset scheduledTime,
        CancellationToken cancellationToken = default)
        where TEvent : IAmAnEvent
    {
        return EnqueueAndFlush(message, scheduledTime, cancellationToken);
    }

    private async Task EnqueueAndFlush<TMessage>(TMessage message,
        CancellationToken cancellationToken)
        where TMessage : IAmAMessage
    {
        await Enqueue(message, cancellationToken).ConfigureAwait(false);
        await emitter.FlushAll(this, cancellationToken).ConfigureAwait(false);
    }

    private async Task EnqueueAndFlush<TMessage>(TMessage message, DateTimeOffset scheduledTime,
        CancellationToken cancellationToken)
        where TMessage : IAmAMessage
    {
        await Enqueue(message, scheduledTime, cancellationToken).ConfigureAwait(false);
        await emitter.FlushAll(this, cancellationToken).ConfigureAwait(false);
    }
}