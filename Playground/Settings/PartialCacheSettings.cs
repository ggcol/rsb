using ASureBus.Abstractions.Configurations;

namespace Playground.Settings;

public class PartialCacheSettings : IConfigureAsbCache
{
    public TimeSpan? Expiration { get; set; }
    public string? TopicConfigPrefix { get; set; }
    public string? ServiceBusSenderCachePrefix { get; set; }
}