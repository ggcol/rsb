using System.Timers;
using Timer = System.Timers.Timer;

namespace Rsb.Core.Caching.Entities;

internal sealed class ObservableCacheItem
{
    internal object Key { get; }
    internal object? CachedValue { get; }
    internal bool HasExpiration => _expiresAfter is not null;
    internal event EventHandler? Expired;

    private readonly TimeSpan? _expiresAfter;
    private Timer? _timer;

    public ObservableCacheItem(object key, object? cachedValue,
        TimeSpan? expiresAfter = null)
    {
        Key = key;
        CachedValue = cachedValue;
        _expiresAfter = expiresAfter;

        if (!HasExpiration) return;

        SetTimer();
    }

    private void SetTimer()
    {
        _timer = new Timer(_expiresAfter!.Value.TotalMilliseconds);
        _timer.Elapsed += OnExpired;
        _timer.AutoReset = false;
        _timer.Start();
    }

    private void OnExpired(object? o, ElapsedEventArgs e)
    {
        Expired?.Invoke(this, EventArgs.Empty);
        _timer?.Dispose();
    }
}