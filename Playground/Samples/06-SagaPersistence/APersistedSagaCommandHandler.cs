using Rsb;

namespace Playground.Samples._06_SagaPersistence;

public class APersistedSagaCommandHandler
    : IHandleMessage<APersistedSagaCommand>
{
    public async Task Handle(APersistedSagaCommand message,
        IMessagingContext context,
        CancellationToken cancellationToken = default)
    {
        Console.WriteLine($"========== {nameof(APersistedSagaCommand)} ==========");
        Console.WriteLine($"{nameof(APersistedSagaCommand)} correlationId: {context.CorrelationId}");
        
        await context.Send(new APersistedSagaReply("world!"), cancellationToken)
            .ConfigureAwait(false);
    }
}