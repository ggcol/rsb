using Rsb.Core.TypesHandling.Entities;

namespace Rsb.Services.SqlServer;

internal class SagaSqlServerPersistenceService(ISqlServerService storage) 
    : ISagaPersistenceService
{
    public async Task<object?> Get(SagaType sagaType, Guid correlationId,
        CancellationToken cancellationToken = default)
    {
        return await storage.Get(sagaType, correlationId, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task Save<TItem>(TItem item, SagaType sagaType, Guid correlationId,
        CancellationToken cancellationToken = default)
    {
        await storage.Save(item, sagaType, correlationId, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task Delete(SagaType sagaType, Guid correlationId,
        CancellationToken cancellationToken = default)
    {
        await storage.Delete(sagaType, correlationId, cancellationToken)
            .ConfigureAwait(false);
    }
}