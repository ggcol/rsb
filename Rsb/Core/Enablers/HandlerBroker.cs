using System.Text.Json;
using Rsb.Core.Enablers.Entities;
using Rsb.Services;

namespace Rsb.Core.Enablers;

internal sealed class HandlerBroker<TMessage> : IHandlerBroker<TMessage>
    where TMessage : IAmAMessage
{
    public ICollectMessage Collector => (ICollectMessage)_context;
    private readonly IHandleMessage<TMessage> _handler;
    private readonly IMessagingContext _context;

    public HandlerBroker(IHandleMessage<TMessage> handler, IMessagingContext context)
    {
        _context = context;
        _handler = handler;
    }

    public async Task Handle(BinaryData binaryData,
        CancellationToken cancellationToken)
    {
        var message = await Deserialize(binaryData);

        await _handler.Handle(message.Message, _context, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task HandleError(Exception ex,
        CancellationToken cancellationToken)
    {
        await _handler.HandleError(ex, _context, cancellationToken)
            .ConfigureAwait(false);
    }

    private static async Task<RsbMessage<TMessage>?> Deserialize(BinaryData binaryData)
    {
        return await JsonSerializer
            .DeserializeAsync<RsbMessage<TMessage>>(binaryData.ToStream())
            .ConfigureAwait(false);
    }
}