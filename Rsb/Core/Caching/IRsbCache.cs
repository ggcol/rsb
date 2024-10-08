﻿namespace Rsb.Core.Caching;

internal interface IRsbCache
{
    internal object? Set(object key, object? obj, TimeSpan? expiresAfter = null);
    internal T? Set<T>(object key, T? obj, TimeSpan? expiresAfter = null);
    internal bool TryGetValue(object key, out object? retrieved);
    internal bool TryGetValue<T>(object key, out T? retrieved);
    internal void Remove(object key);
}