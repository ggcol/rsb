// ReSharper disable InconsistentNaming
namespace ASureBus.Configurations.ConfigObjects;

public sealed class AsbCacheConfig : IConfigureAsbCache
{
    private static readonly TimeSpan DEFAULT_EXPIRATION =
        TimeSpan.FromMinutes(5);
    private const string DEFAULT_TOPIC_CONFIG_PREFIX = "topicConfig";
    private const string DEFAULT_SERVICEBUS_SENDER_PREFIX = "sender";

    private TimeSpan? _expiration = DEFAULT_EXPIRATION;
    private string? _topicConfigPrefix = DEFAULT_TOPIC_CONFIG_PREFIX;
    private string? _serviceBusSenderCachePrefix = DEFAULT_SERVICEBUS_SENDER_PREFIX;

    public TimeSpan? Expiration
    {
        get => _expiration ?? DEFAULT_EXPIRATION;
        set => _expiration = value;
    }

    public string? TopicConfigPrefix
    {
        get => _topicConfigPrefix ?? DEFAULT_TOPIC_CONFIG_PREFIX;
        set => _topicConfigPrefix = value;
    }

    public string? ServiceBusSenderCachePrefix
    {
        get => _serviceBusSenderCachePrefix ?? DEFAULT_SERVICEBUS_SENDER_PREFIX;
        set => _serviceBusSenderCachePrefix = value;
    }
}
