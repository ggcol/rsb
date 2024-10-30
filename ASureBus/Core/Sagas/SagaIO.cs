using ASureBus.Core.TypesHandling.Entities;
using ASureBus.Services;
using ASureBus.Services.SqlServer;
using ASureBus.Services.StorageAccount;

namespace ASureBus.Core.Sagas;

// ReSharper disable once InconsistentNaming
internal class SagaIO : ISagaIO
{
    private readonly ISagaPersistenceService _persistenceService = MakeStorageService();

    private static ISagaPersistenceService MakeStorageService()
    {
        if (RsbConfiguration.UseDataStorageSagaPersistence)
        {
            return new SagaDataStoragePersistenceService(
                new AzureDataStorageService(
                    RsbConfiguration.DataStorageSagaPersistence?.DataStorageConnectionString));
        }
        else if (RsbConfiguration.UseSqlServerSagaPersistence)
        {
            return new SagaSqlServerPersistenceService(new SqlServerService());
        }
        else
        {
            throw new NotImplementedException(); //customize
        }
    }

    public async Task<object?> Load(Guid correlationId, SagaType sagaType,
        CancellationToken cancellationToken = default)
    {
        return await _persistenceService.Get(sagaType, correlationId, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task Unload(object? implSaga, Guid correlationId,
        SagaType sagaType, CancellationToken cancellationToken = default)
    {
        await _persistenceService.Save(implSaga, sagaType, correlationId, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task Delete(Guid correlationId, SagaType sagaType,
        CancellationToken cancellationToken = default)
    {
        await _persistenceService.Delete(sagaType, correlationId, cancellationToken)
            .ConfigureAwait(false);
    }
}