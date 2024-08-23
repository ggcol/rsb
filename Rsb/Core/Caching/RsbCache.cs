using Rsb.Core.Caching.Entities;

namespace Rsb.Core.Caching;

internal sealed class RsbCache : IRsbCache
{
    private readonly IDictionary<object, ObservableCacheItem?> _shelf
        = new Dictionary<object, ObservableCacheItem?>();

    public T? Set<T>(object key, T? obj, TimeSpan? expiresAfter = null)
    {
        var obs = new ObservableCacheItem(key, obj, expiresAfter);
        
        if (obs.HasExpiration)
        {
            obs.Expired += (_, _) => 
            {
                _shelf.Remove(obs.Key);
            };
        }

        _shelf.Add(key, obs);
        return obj;
    }
    
    public object? Set(object key, object? obj,
        TimeSpan? expiresAfter = null)
    {
        return Set<object>(key, obj, expiresAfter);
    }

    public bool TryGetValue<T>(object key, out T? retrieved)
    {
        var exists = _shelf.TryGetValue(key, out var obs);

        if (exists)
        {
            retrieved = (T?)obs?.CachedValue;
            return exists;
        }

        retrieved = default;
        return false;
    }
    
    public bool TryGetValue(object key, out object? retrieved)
    {
        return TryGetValue<object>(key, out retrieved);
    }

    public void Remove(object key)
    {
        _shelf.Remove(key);
    }
}