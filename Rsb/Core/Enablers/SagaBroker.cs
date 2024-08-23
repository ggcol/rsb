using System.Text.Json;
using Rsb.Core.Enablers.Entities;
using Rsb.Core.Messaging;

namespace Rsb.Core.Enablers;

internal sealed class SagaBroker<TSagaData, TMessage> : ISagaBroker
    where TSagaData : SagaData, new()
    where TMessage : IAmAMessage
{
    public ICollectMessage Collector => (ICollectMessage)_context;
    private readonly Saga<TSagaData> _saga;
    private readonly IMessagingContext _context;

    public SagaBroker(Saga<TSagaData> saga,
        IMessagingContext context)
    {
        _saga = saga;
        _context = context;
    }

    public async Task Handle(BinaryData binaryData,
        CancellationToken cancellationToken)
    {
        var method = _saga
            .GetType()
            .GetMethods()
            /*
             * TODO this nameof() is misleading, it doesn't refers directly to
             * Saga<>.Handle
             */
            .FirstOrDefault(m => m.Name.Equals(nameof(Handle)) &&
                                 m.GetParameters().Length == 3 &&
                                 m.GetParameters()[0].ParameterType ==
                                 typeof(TMessage)
            );

        var rsbMessage = await Deserialize(binaryData, cancellationToken);

        if (method is not null)
        {
            await (Task)method.Invoke(_saga,
                new object[] { rsbMessage.Message, _context, cancellationToken });
        }
    }

    public async Task HandleError(Exception ex,
        CancellationToken cancellationToken)
    {
        var method = _saga
            .GetType()
            .GetMethods()
            /*
             * TODO this nameof() is misleading, it doesn't refers directly to
             * Saga<>.Handle
             */
            .FirstOrDefault(m => m.Name.Equals(nameof(HandleError)) &&
                                 m.GetParameters().Length == 3 &&
                                 m.GetParameters()[0].ParameterType ==
                                 typeof(TMessage)
            );

        if (method is not null)
        {
            await (Task)method.Invoke(_saga,
                new object[] { ex, _context, cancellationToken });
        }
    }

    private static async Task<RsbMessage<TMessage>?> Deserialize(BinaryData binaryData,
        CancellationToken cancellationToken)
    {
        return await JsonSerializer
            .DeserializeAsync<RsbMessage<TMessage>>(binaryData.ToStream(),
                cancellationToken: cancellationToken)
            .ConfigureAwait(false);
    }
}