using System.Text.Json;
using Rsb.Core;

namespace Rsb.Services.StorageAccount;

internal class HeavyPropsStorageService() 
    : AzureDataStorageService(RsbConfiguration.HeavyProps?.DataStorageConnectionString!)
{
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
    
    private static object? Deserialize(string read, Type returnType)
    {
        return JsonSerializer.Deserialize(read, returnType);
    }
}