using ASureBus.Abstractions;
using Microsoft.Extensions.Logging;
using Playground.Samples._06_SagaPersistence.Messages;

namespace Playground.Samples._06_SagaPersistence;

public class APersistedSagaCommandHandler(ILogger<APersistedSagaCommandHandler> logger)
    : IHandleMessage<APersistedSagaCommand>
{
    public async Task Handle(APersistedSagaCommand message,
        IMessagingContext context,
        CancellationToken cancellationToken = default)
    {
        var messageName = message.GetType().Name;
        
        logger.LogInformation("========== {MessageName} ==========", messageName);
        
        logger.LogInformation("{MessageName} correlationId: {context.CorrelationId}", 
            messageName, context.CorrelationId);
        
        await context.Send(new APersistedSagaReply("world!"), cancellationToken)
            .ConfigureAwait(false);
    }
}