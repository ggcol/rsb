using System.Text;
using System.Text.Json;
using Azure.Storage.Blobs;
using Rsb.Configurations;

namespace Rsb.Services.StorageAccount;

public class AzureBlobStorageService : IAzureBlobStorageService
{
    public async Task Save<TItem>(TItem item, string containerName,
        string blobName, bool overwrite = default,
        CancellationToken cancellationToken = default)
    {
        var containerClient =
            await MakeContainerClient(containerName, cancellationToken)
                .ConfigureAwait(false);
        var blobClient = containerClient.GetBlobClient(blobName);

        /*
         * TODO
         * For ease of test serialization is now json,
         * but option to use bytes should be given!
         */

        var jsonString = JsonSerializer.Serialize(item);

        // using var serializationStream = new MemoryStream();
        // var serializer = new DataContractSerializer(typeof(TItem));
        // serializer.WriteObject(serializationStream, item);
        // var itemBytes = serializationStream.ToArray();

        var itemBytes = Encoding.UTF8.GetBytes(jsonString);

        using var uploadStream = new MemoryStream(itemBytes);
        await blobClient.UploadAsync(uploadStream, overwrite, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<string> Get(string containerName, string blobName,
        CancellationToken cancellationToken = default)
    {
        var containerClient =
            await MakeContainerClient(containerName, cancellationToken)
                .ConfigureAwait(false);
        var blobClient = containerClient.GetBlobClient(blobName);

        var downloadInfo = await blobClient
            .OpenReadAsync(cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        using var reader = new StreamReader(downloadInfo);
        return await reader.ReadToEndAsync(cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task Delete(string containerName, string blobName,
        CancellationToken cancellationToken = default)
    {
        var containerClent =
            await MakeContainerClient(containerName, cancellationToken)
                .ConfigureAwait(false);
        var blobClient = containerClent.GetBlobClient(blobName);

        _ = await blobClient
            .DeleteAsync(cancellationToken: cancellationToken)
            .ConfigureAwait(false);

        //return?
    }

    private async Task<BlobContainerClient> MakeContainerClient(
        string containerName, CancellationToken cancellationToken)
    {
        var containerClient =
            MakeServiceClient().GetBlobContainerClient(containerName);

        if (!await containerClient.ExistsAsync(cancellationToken)
                .ConfigureAwait(false))
        {
            await containerClient
                .CreateAsync(cancellationToken: cancellationToken)
                .ConfigureAwait(false);
        }

        return containerClient;
    }

    private BlobServiceClient MakeServiceClient()
    {
        return new BlobServiceClient(RsbConfiguration.HeavyProps
            ?.DataStorageConnectionString);
    }
}