using Rsb.Configurations;

namespace Rsb;

public interface IHandleMessage<in TMessage>
    // : IHandleMessage 
    where TMessage : IAmAMessage
{
    //TODO nullable?
    public Task Handle(TMessage? message, IMessagingContext context);
    
    public Task HandleErrors(Exception ex, IMessagingContext context)
    {
        throw ex;
    }
}

// public interface IHandleMessage
// {
//     public Task Handle(IAmAMessage message, IMessagingContext context);
//     
//     public Task HandleErrors(Exception ex, IMessagingContext context)
//     {
//         throw ex;
//     }
// }