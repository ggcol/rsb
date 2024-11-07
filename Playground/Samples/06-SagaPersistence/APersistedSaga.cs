using ASureBus.Abstractions;
using Microsoft.Extensions.Logging;
using Playground.Samples._06_SagaPersistence.Messages;

namespace Playground.Samples._06_SagaPersistence;

public class APersistedSaga(ILogger<APersistedSaga> logger) : Saga<APersistedSagaData>,
    IAmStartedBy<APersistedSagaInitCommand>,
    IHandleMessage<APersistedSagaReply>
{
    public async Task Handle(APersistedSagaInitCommand message,
        IMessagingContext context,
        CancellationToken cancellationToken)
    {
        var messageName = message.GetType().Name;

        SagaData.AProp = "Hello";
        
        logger.LogInformation("========== {MessageName} ==========", messageName);
        
        logger.LogInformation("{MessageName} received, correlationId: {CorrelationId}", 
            messageName, context.CorrelationId);

        await context.Send(new APersistedSagaCommand(), cancellationToken)
            .ConfigureAwait(false);
    }

    public Task Handle(APersistedSagaReply message, IMessagingContext context,
        CancellationToken cancellationToken)
    {
        var messageName = message.GetType().Name;
        
        SagaData.AProp += $" {message.Something}";

        logger.LogInformation("========== {MessageName} ==========", messageName);
        
        logger.LogInformation("{MessageName} received, correlationId: {CorrelationId}", 
            messageName, context.CorrelationId);

        logger.LogInformation("Print saga data: {SagaData}", SagaData.AProp);

        IAmComplete();
        return Task.CompletedTask;
    }
}