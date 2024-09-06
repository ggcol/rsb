using System.Text;
using System.Text.Json;
using Azure.Storage.Blobs;

namespace Rsb.Services.StorageAccount;

internal abstract class AzureDataStorageService(string connectionString)
{
    public async Task Save<TItem>(TItem item, string containerName,
        string blobName, bool overwrite = default,
        CancellationToken cancellationToken = default)
    {
        var containerClient =
            await MakeContainerClient(containerName, cancellationToken)
                .ConfigureAwait(false);
        var blobClient = containerClient.GetBlobClient(blobName);

        var itemBytes = Serialize(item);

        using var uploadStream = new MemoryStream(itemBytes);
        await blobClient.UploadAsync(uploadStream, overwrite, cancellationToken)
            .ConfigureAwait(false);
    }

    private static byte[] Serialize<TItem>(TItem item)
    {
        var jsonString = JsonSerializer.Serialize(item);
        return Encoding.UTF8.GetBytes(jsonString);
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
    protected async Task<BlobContainerClient> MakeContainerClient(
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
        return new BlobServiceClient(connectionString);
    }
}