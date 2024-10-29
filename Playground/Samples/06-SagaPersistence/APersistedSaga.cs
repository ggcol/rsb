using Asb.Abstractions;

namespace Playground.Samples._06_SagaPersistence;

public class APersistedSaga : Saga<APersistedSagaData>,
    IAmStartedBy<APersistedSagaInitCommand>,
    IHandleMessage<APersistedSagaReply>
{
    public async Task Handle(APersistedSagaInitCommand message,
        IMessagingContext context,
        CancellationToken cancellationToken = default)
    {
        SagaData.AProp = "Hello";

        Console.WriteLine($"========== {nameof(APersistedSagaInitCommand)} ==========");
        Console.WriteLine($"{nameof(APersistedSagaInitCommand)} correlationId: {context.CorrelationId}");

        await context.Send(new APersistedSagaCommand(), cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task Handle(APersistedSagaReply message,
        IMessagingContext context,
        CancellationToken cancellationToken = default)
    {
        SagaData.AProp += $" {message.Something}";

        Console.WriteLine($"========== {nameof(APersistedSagaReply)} ==========");
        Console.WriteLine($"{nameof(APersistedSagaReply)} correlationId: {context.CorrelationId}");

        Console.WriteLine(SagaData.AProp);
        
        IAmComplete();
    }
}