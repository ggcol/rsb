using Rsb.Core.TypesHandling.Entities;
using Rsb.Services;
using Rsb.Services.StorageAccount;

namespace Rsb.Core.Sagas;

// ReSharper disable once InconsistentNaming
internal class SagaIO(ISagaPersistenceService storage) : ISagaIO
{
    public async Task<object?> Load(Guid correlationId, SagaType sagaType,
        CancellationToken cancellationToken = default)
    {
        return await storage.Get(sagaType, correlationId, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task Unload(object? implSaga, Guid correlationId,
        SagaType sagaType, CancellationToken cancellationToken = default)
    {
        await storage.Save(implSaga, sagaType, correlationId, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task Delete(Guid correlationId, SagaType sagaType,
        CancellationToken cancellationToken = default)
    {
        await storage.Delete(sagaType, correlationId, cancellationToken)
            .ConfigureAwait(false);
    }
}