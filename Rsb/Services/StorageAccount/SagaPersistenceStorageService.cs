using Rsb.Core;
using Rsb.Core.TypesHandling.Entities;
using Rsb.Utils;

namespace Rsb.Services.StorageAccount;

internal class SagaPersistenceStorageService()
    : AzureDataStorageService(RsbConfiguration.SagaPersistence
        ?.DataStorageConnectionString!)
{
    public async Task<object?> Get(string containerName, string blobName,
        SagaType sagaType, CancellationToken cancellationToken = default)
    {
        var containerClient =
            await MakeContainerClient(containerName, cancellationToken)
                .ConfigureAwait(false);
        var blobClient = containerClient.GetBlobClient(blobName);

        var downloadInfo = await blobClient
            .OpenReadAsync(cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        using var reader = new StreamReader(downloadInfo);
        var read = await reader.ReadToEndAsync(cancellationToken)
            .ConfigureAwait(false);

        return Serializer.Deserialize(read, sagaType.Type,
            new SagaConverter(sagaType.Type, sagaType.SagaDataType));
    }
}