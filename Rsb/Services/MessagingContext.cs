using Rsb.Configurations;

namespace Rsb.Services;

internal sealed class MessagingContext : IMessagingContext, ICollectMessage
{
    public Queue<ICollectMessage.MessageHolder> Messages { get; } = new();

    public async Task Send<TCommand>(TCommand message,
        CancellationToken cancellationToken = default)
        where TCommand : IAmACommand
    {
        Messages.Enqueue(new()
        {
            MessageName = typeof(TCommand).Name,
            Message = message
        });
    }

    public async Task Publish<TEvent>(TEvent message,
        CancellationToken cancellationToken = default)
        where TEvent : IAmAnEvent
    {
        Messages.Enqueue(new()
        {
            MessageName = typeof(TEvent).Name,
            Message = message
        });
    }
}