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

        var itemBytes = Serialize(item);

        using var uploadStream = new MemoryStream(itemBytes);
        await blobClient.UploadAsync(uploadStream, overwrite, cancellationToken)
            .ConfigureAwait(false);
    }

    public async Task<object?> Get(string containerName, string blobName,
        Type returnType, CancellationToken cancellationToken = default)
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

        return Deserialize(read, returnType);
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

    private static byte[] Serialize<TItem>(TItem item)
    {
        var jsonString = JsonSerializer.Serialize(item);
        return Encoding.UTF8.GetBytes(jsonString);
    }

    private static object? Deserialize(string read, Type returnType)
    {
        return JsonSerializer.Deserialize(read, returnType);
    }
}