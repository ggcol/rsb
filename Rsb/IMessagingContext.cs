namespace Rsb;

public interface IMessagingContext
{
    public Guid CorrelationId { get; }
    
    public Task Send<TMessage>(TMessage message,
        CancellationToken cancellationToken = default)
        where TMessage : IAmACommand;

    public Task Publish<TEvent>(TEvent message,
        CancellationToken cancellationToken = default)
        where TEvent : IAmAnEvent;
}