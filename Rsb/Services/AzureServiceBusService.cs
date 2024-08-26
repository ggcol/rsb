using System.Reflection;
using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using Microsoft.Extensions.Options;
using Rsb.Configurations;
using Rsb.Core.Caching;
using Rsb.Services.Options;

namespace Rsb.Services;

internal sealed class AzureServiceBusService<TSettings>
    : IAzureServiceBusService
    where TSettings : class, IConfigureAzureServiceBus, new()
{
    private ServiceBusClient _sbClient { get; }
    private readonly IOptions<TSettings> _options;
    private readonly AzureServiceBusServiceOptions _myOptions = new();
    private readonly IRsbCache _cache;

    public AzureServiceBusService(IOptions<TSettings> options,
        IRsbCache cache)
    {
        _options = options;
        _cache = cache;

        _sbClient = new ServiceBusClient(
            _options.Value.ConnectionString,
            _myOptions.SbClientOptions);
    }

    public async Task<string> ConfigureQueue(string queueName, 
        CancellationToken cancellationToken = default)
    {
        if (_cache.TryGetValue(queueName, out string queue)) return queue;

        var admClient = MakeAdmClient();

        if (!await admClient.QueueExistsAsync(queueName, cancellationToken))
        {
            var rx = await admClient.CreateQueueAsync(
                queueName, cancellationToken);
            queueName = rx.Value.Name;
        }

        return _cache.Set(queueName, queueName,
            _myOptions.CacheOptions.RsbCacheDefaultExpiration);
    }

    public async Task<string> ConfigureTopicForSender(string topicName, 
        CancellationToken cancellationToken = default)
    {
        if (_cache.TryGetValue(topicName, out string topic)) return topic;

        var admClient = MakeAdmClient();

        if (!await admClient.TopicExistsAsync(topicName, cancellationToken))
        {
            //A message sent to a topic without subscription is lost :/
            var rx = await admClient.CreateTopicAsync(
                topicName, cancellationToken);
            topicName = rx.Value.Name;
        }

        return _cache.Set(topicName, topicName,
            _myOptions.CacheOptions.RsbCacheDefaultExpiration);
    }

    public async Task<TopicConfiguration> ConfigureTopicForReceiver(
        MemberInfo messageType, CancellationToken cancellationToken = default)
    {
        var config = new TopicConfiguration(messageType.Name,
            Assembly.GetEntryAssembly()?.GetName().Name);

        var cacheKey = CacheKey(_myOptions.CacheOptions.TopicConfigCachePrefix,
            config.Name);

        if (_cache.TryGetValue(cacheKey, out TopicConfiguration cachedConfig))
            return cachedConfig;

        var admClient = MakeAdmClient();

        if (!await admClient.TopicExistsAsync(config.Name, cancellationToken))
        {
            var rx = await admClient.CreateTopicAsync(
                config.Name, cancellationToken);
        }

        if (!await admClient.SubscriptionExistsAsync(config.Name,
                config.SubscriptionName, cancellationToken))
        {
            var rxSub = await admClient
                .CreateSubscriptionAsync(config.Name, config.SubscriptionName, 
                    cancellationToken);
        }

        return _cache.Set(cacheKey, config,
            _myOptions.CacheOptions.RsbCacheDefaultExpiration);
    }

    //TODO store? throwaway?
    private ServiceBusAdministrationClient MakeAdmClient()
    {
        return new ServiceBusAdministrationClient(_options.Value
            .ConnectionString);
    }

    public ServiceBusSender GetSender(string destination)
    {
        var cacheKey =
            CacheKey(_myOptions.CacheOptions.ServiceBusSenderCachePrefix,
                destination);

        return _cache.TryGetValue(cacheKey, out ServiceBusSender sender)
            ? sender
            : _cache.Set(cacheKey, _sbClient.CreateSender(destination),
                _myOptions.CacheOptions.RsbCacheDefaultExpiration);
    }

    public ServiceBusProcessor GetProcessor(string queueName)
    {
        //TODO check this
        //It does not make sense to cache processors
       return _sbClient.CreateProcessor(queueName);
    }

    public ServiceBusProcessor GetProcessor(string topicName,
        string subscriptionName)
    {
        //TODO check this
        //It does not make sense to cache processors
        return _sbClient.CreateProcessor(topicName, subscriptionName);
    }

    private string CacheKey(params string[] values)
    {
        return string.Join("-", values.Where(x => !string.IsNullOrEmpty(x)));
    }
}

public sealed record TopicConfiguration(string Name, string SubscriptionName);