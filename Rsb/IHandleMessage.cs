namespace Rsb;

public interface IHandleMessage<in TMessage>
{
    public Task Handle(TMessage message, IMessagingContext context);
    
    public Task HandleErrors(Exception ex, IMessagingContext context)
    {
        throw ex;
    }
}