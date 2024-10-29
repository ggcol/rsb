using Asb.Configurations;

namespace Playground.Settings;

public class WholeCacheSettings : IConfigureRsbCache
{
    public TimeSpan? Expiration { get; set; }
    public string? TopicConfigPrefix { get; set; }
    public string? ServiceBusSenderCachePrefix { get; set; }
}

public class PartialCacheSettings : IConfigureRsbCache
{
    public TimeSpan? Expiration { get; set; }
    public string TopicConfigPrefix { get; set; }
    public string ServiceBusSenderCachePrefix { get; set; }
}