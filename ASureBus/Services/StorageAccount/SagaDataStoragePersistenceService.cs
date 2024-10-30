using ASureBus.Core;
using ASureBus.Core.Sagas;
using ASureBus.Core.TypesHandling.Entities;

namespace ASureBus.Services.StorageAccount;

internal sealed class SagaDataStoragePersistenceService(
    IAzureDataStorageService storage) : ISagaPersistenceService
{
    public async Task<object?> Get(SagaType sagaType, Guid correlationId,
        CancellationToken cancellationToken = default)
    {
        return await storage.Get(
                RsbConfiguration.DataStorageSagaPersistence?.DataStorageContainer!,
                GetName(sagaType, correlationId),
                sagaType.Type,
                new SagaConverter(sagaType.Type, sagaType.SagaDataType),
                cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task Save<TItem>(TItem item, SagaType sagaType,
        Guid correlationId,
        CancellationToken cancellationToken = default)
    {
        await storage.Save(
                item,
                RsbConfiguration.DataStorageSagaPersistence?.DataStorageContainer!,
                GetName(sagaType, correlationId),
                true,
                cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task Delete(SagaType sagaType, Guid correlationId,
        CancellationToken cancellationToken = default)
    {
        await storage.Delete(
                RsbConfiguration.DataStorageSagaPersistence?.DataStorageContainer!,
                GetName(sagaType, correlationId),
                cancellationToken)
            .ConfigureAwait(false);
    }

    private static string GetName(SagaType sagaType, Guid correlationId)
    {
        return $"{sagaType.Type.Name}-{correlationId}";
    }
}