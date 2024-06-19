using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Caching.Memory;

namespace rsb.Services.Options;

public class AzureServiceBusServiceOptions
{
    public readonly ServiceBusClientOptions SbClientOptions = new()
    {
        TransportType = ServiceBusTransportType.AmqpWebSockets
    };

    public CacheOptions CacheOptions { get; set; } = new();
}

public class CacheOptions
{
    public readonly MemoryCacheEntryOptions DefaultCacheEntriesOptions = new()
    {
        AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
    };
    
    public string TopicConfigCachePrefix = "topicConfig";
    public string ServiceBusSenderCachePrefix = "sender";
}