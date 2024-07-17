using System.Text.Json;
using Rsb.Configurations;
using Rsb.Services;

namespace Rsb.Enablers;

internal sealed class ListenerBroker<T> : IListenerBroker<T> where T : IAmAMessage
{
    public ICollectMessage Collector => (ICollectMessage)_context;
    private readonly IHandleMessage<T> _handler;
    private readonly IMessagingContext _context;

    public ListenerBroker(IHandleMessage<T> handler, IMessagingContext context)
    {
        _context = context;
        _handler = handler;
    }

    public async Task Handle(BinaryData binaryData,
        CancellationToken cancellationToken)
    {
        //TODO check if async is ok
        var message = await Deserialize(binaryData);

        await _handler.Handle(message, _context, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task HandleError(Exception ex,
        CancellationToken cancellationToken)
    {
        await _handler.HandleError(ex, _context, cancellationToken)
            .ConfigureAwait(false);
    }

    private static async Task<T?> Deserialize(BinaryData binaryData)
    {
        return await JsonSerializer
            .DeserializeAsync<T>(binaryData.ToStream())
            .ConfigureAwait(false);
    }
}