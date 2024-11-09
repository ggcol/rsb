using ASureBus.Abstractions;
using ASureBus.Core.Entities;

namespace ASureBus.Core.Enablers;

internal sealed class HandlerBroker<TMessage>(
    IHandleMessage<TMessage> handler, 
    IMessagingContext context)
    : BrokerBehavior<TMessage>(context), IHandlerBroker<TMessage>
    where TMessage : IAmAMessage
{
    public async Task<IAsbMessage> Handle(BinaryData binaryData,
        CancellationToken cancellationToken = default)
    {
        var asbMessage = await GetFrom(binaryData, cancellationToken);

        await handler.Handle(asbMessage.Message, _context, cancellationToken)
            .ConfigureAwait(false);
        
        return asbMessage;
    }

    public async Task HandleError(Exception ex,
        CancellationToken cancellationToken = default)
    {
        await handler.HandleError(ex, _context, cancellationToken)
            .ConfigureAwait(false);
    }
}