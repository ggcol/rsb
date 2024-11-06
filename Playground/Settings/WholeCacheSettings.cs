using ASureBus.Configurations;

namespace Playground.Settings;

public class WholeCacheSettings : IConfigureAsbCache
{
    public TimeSpan? Expiration { get; set; }
    public string? TopicConfigPrefix { get; set; }
    public string? ServiceBusSenderCachePrefix { get; set; }
}

public class PartialCacheSettings : IConfigureAsbCache
{
    public TimeSpan? Expiration { get; set; }
    public string TopicConfigPrefix { get; set; }
    public string ServiceBusSenderCachePrefix { get; set; }
}