namespace Rsb;

public interface IMessagingContext
{
    public Guid CorrelationId { get; }
    
    public Task Send<TCommand>(TCommand message,
        CancellationToken cancellationToken = default)
        where TCommand : IAmACommand;

    public Task Publish<TEvent>(TEvent message,
        CancellationToken cancellationToken = default)
        where TEvent : IAmAnEvent;
}