namespace ASureBus.Abstractions;

public interface IHandleMessage<in TMessage>
    where TMessage : IAmAMessage
{
    public Task Handle(TMessage message, IMessagingContext context,
        CancellationToken cancellationToken);

    public Task HandleError(Exception ex, IMessagingContext context,
        CancellationToken cancellationToken)
    {
        throw ex;
    }
}