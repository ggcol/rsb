namespace ASureBus.Core.Caching;

internal interface IAsbCache
{
    internal object? Set(object key, object? obj, TimeSpan? expiresAfter = null);
    internal T? Set<T>(object key, T? obj, TimeSpan? expiresAfter = null);
    internal bool TryGetValue(object key, out object? retrieved);
    internal bool TryGetValue<T>(object key, out T? retrieved);
    internal void Remove(object key);
    internal T? Upsert<T>(object key, T? obj, TimeSpan? expiresAfter = null);
    internal object? Upsert(object key, object? obj, TimeSpan? expiresAfter = null);
}