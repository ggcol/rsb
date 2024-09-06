using Rsb.Configurations;
using Rsb.Core.TypesHandling.Entities;
using Rsb.Services.StorageAccount;

namespace Rsb.Core.Sagas;

// ReSharper disable once InconsistentNaming
internal class SagaIO()
    : ISagaIO
{
    private readonly SagaPersistenceStorageService _storageService = new();

    public async Task<object?> Load(Guid correlationId, SagaType sagaType,
        CancellationToken cancellationToken = default)
    {
        return await _storageService.Get(
                RsbConfiguration.SagaPersistence?.DataStorageContainer!,
                correlationId.ToString(),
                sagaType, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task Unload(object? implSaga, Guid correlationId,
        CancellationToken cancellationToken = default)
    {
        await _storageService.Save(
                implSaga,
                RsbConfiguration.SagaPersistence?.DataStorageContainer!,
                correlationId.ToString(),
                true, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task Delete(Guid correlationId,
        CancellationToken cancellationToken = default)
    {
        await _storageService.Delete(
                RsbConfiguration.SagaPersistence?.DataStorageContainer!,
                correlationId.ToString(), cancellationToken)
            .ConfigureAwait(false);
    }
}