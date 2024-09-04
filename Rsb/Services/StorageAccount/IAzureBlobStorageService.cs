namespace Rsb.Services.StorageAccount;

public interface IAzureBlobStorageService
{
    public Task Save<TItem>(TItem item, string containerName,
        string blobName, bool overwrite = default,
        CancellationToken cancellationToken = default);

    public Task<object?> Get(string containerName, string blobName,
        Type returnType, CancellationToken cancellationToken = default);

    public Task Delete(string containerName, string blobName,
        CancellationToken cancellationToken = default);
}