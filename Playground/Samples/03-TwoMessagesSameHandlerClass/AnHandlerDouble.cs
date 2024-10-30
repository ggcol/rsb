using ASureBus.Abstractions;

namespace Playground.Samples._03_TwoMessagesSameHandlerClass;

public class AnHandlerDouble : IHandleMessage<Message1>,
    IHandleMessage<Message2>
{
    public Task Handle(Message1 message, IMessagingContext context,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }

    public Task Handle(Message2 message, IMessagingContext context,
        CancellationToken cancellationToken = default)
    {
        throw new NotImplementedException();
    }
}