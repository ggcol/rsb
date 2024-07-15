using System.Reflection;
using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using Rsb.Configurations;
using Rsb.Services.Options;

namespace Rsb.Services;

internal sealed class AzureServiceBusService<TSettings>
    : IAzureServiceBusService
    where TSettings : class, IConfigureAzureServiceBus, new()
{
    private ServiceBusClient _sbClient { get; }
    private readonly IOptions<TSettings> _options;
    private readonly AzureServiceBusServiceOptions _myOptions = new();
    private readonly IMemoryCache _cache;

    public AzureServiceBusService(IOptions<TSettings> options,
        IMemoryCache cache)
    {
        _options = options;
        _cache = cache;

        _sbClient = new ServiceBusClient(_options.Value.SbConnectionString,
            _myOptions.SbClientOptions);
    }

    public async Task<string> ConfigureQueue(string queueName)
    {
        if (_cache.TryGetValue(queueName, out string queue)) return queue;

        var admClient = MakeAdmClient();

        if (!await admClient.QueueExistsAsync(queueName))
        {
            var rx = await admClient.CreateQueueAsync(queueName);
            queueName = rx.Value.Name;
        }

        return _cache.Set(queueName, queueName,
            _myOptions.CacheOptions.DefaultCacheEntriesOptions);
    }

    public async Task<string> ConfigureTopicForSender(string topicName)
    {
        if (_cache.TryGetValue(topicName, out string topic)) return topic;

        var admClient = MakeAdmClient();

        if (!await admClient.TopicExistsAsync(topicName))
        {
            //A message sent to a topic without subscription is lost :/
            var rx = await admClient.CreateTopicAsync(topicName);
            topicName = rx.Value.Name;
        }

        return _cache.Set(topicName, topicName,
            _myOptions.CacheOptions.DefaultCacheEntriesOptions);
    }

    public async Task<TopicConfiguration> ConfigureTopicForReceiver(
        MemberInfo messageType)
    {
        var config = new TopicConfiguration(messageType.Name,
            Assembly.GetEntryAssembly()?.GetName().Name);

        var cacheKey = CacheKey(_myOptions.CacheOptions.TopicConfigCachePrefix,
            config.Name);

        if (_cache.TryGetValue(cacheKey, out TopicConfiguration cachedConfig))
            return cachedConfig;

        var admClient = MakeAdmClient();

        if (!await admClient.TopicExistsAsync(config.Name))
        {
            var rx = await admClient.CreateTopicAsync(config.Name);
        }

        if (!await admClient.SubscriptionExistsAsync(config.Name,
                config.SubscriptionName))
        {
            var rxSub = await admClient
                .CreateSubscriptionAsync(config.Name, config.SubscriptionName);
        }

        return _cache.Set(cacheKey, config,
            _myOptions.CacheOptions.DefaultCacheEntriesOptions);
    }

    //TODO store? throwaway?
    private ServiceBusAdministrationClient MakeAdmClient()
    {
        return new ServiceBusAdministrationClient(_options.Value
            .SbConnectionString);
    }

    public ServiceBusSender GetSender(string destination)
    {
        var cacheKey =
            CacheKey(_myOptions.CacheOptions.ServiceBusSenderCachePrefix,
                destination);

        return _cache.TryGetValue(cacheKey, out ServiceBusSender sender)
            ? sender
            : _cache.Set(cacheKey, _sbClient.CreateSender(destination), 
                _myOptions.CacheOptions.DefaultCacheEntriesOptions);
    }

    public ServiceBusProcessor GetProcessor(string queueOrTopic,
        string subscriptionName = null)
    {
        //TODO check this
        //It does not make sense to cache processors
        return string.IsNullOrEmpty(subscriptionName)
            ? _sbClient.CreateProcessor(queueOrTopic)
            : _sbClient.CreateProcessor(queueOrTopic, subscriptionName);
    }

    private string CacheKey(params string[] values)
    {
        return string.Join("-", values.Where(x => !string.IsNullOrEmpty(x)));
    }
}

public sealed record TopicConfiguration(string Name, string SubscriptionName);