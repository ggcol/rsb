using Azure.Messaging.ServiceBus;

namespace Rsb.Services.Options;

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
    public TimeSpan RsbCacheDefaultExpiration => TimeSpan.FromMinutes(5);

    public string TopicConfigCachePrefix = "topicConfig";
    public string ServiceBusSenderCachePrefix = "sender";
}