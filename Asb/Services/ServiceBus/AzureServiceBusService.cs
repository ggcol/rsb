using System.Reflection;
using Asb.Core;
using Asb.Core.Caching;
using Asb.Core.TypesHandling.Entities;
using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;

namespace Asb.Services.ServiceBus;

internal sealed class AzureServiceBusService(IRsbCache cache)
    : IAzureServiceBusService
{
    private ServiceBusClient _sbClient { get; } = new(
        RsbConfiguration.ServiceBus.ServiceBusConnectionString,
        RsbConfiguration.ServiceBus.ClientOptions);

    public async Task<ServiceBusProcessor> GetProcessor(
        ListenerType handler, CancellationToken cancellationToken = default)
    {
        if (handler.MessageType.IsCommand)
        {
            var queue = await ConfigureQueue(
                    handler.MessageType.Type.Name, cancellationToken)
                .ConfigureAwait(false);

            //TODO check this
            //It does not make sense to cache processors
            return _sbClient.CreateProcessor(queue);
        }

        var topicConfig = await ConfigureTopicForReceiver(
                handler.MessageType.Type, cancellationToken)
            .ConfigureAwait(false);

        //TODO check this
        //It does not make sense to cache processors
        return _sbClient.CreateProcessor(topicConfig.Name,
            topicConfig.SubscriptionName);
    }

    public async Task<string> ConfigureQueue(string queueName,
        CancellationToken cancellationToken = default)
    {
        if (cache.TryGetValue(queueName, out string queue)) return queue;

        var admClient = MakeAdmClient();

        if (!await admClient.QueueExistsAsync(queueName, cancellationToken))
        {
            var rx = await admClient.CreateQueueAsync(
                queueName, cancellationToken);
            queueName = rx.Value.Name;
        }

        return cache.Set(queueName, queueName,
            RsbConfiguration.Cache.Expiration);
    }

    public async Task<string> ConfigureTopicForSender(string topicName,
        CancellationToken cancellationToken = default)
    {
        if (cache.TryGetValue(topicName, out string topic)) return topic;

        var admClient = MakeAdmClient();

        if (!await admClient.TopicExistsAsync(topicName, cancellationToken))
        {
            //A message sent to a topic without subscription is lost :/
            var rx = await admClient.CreateTopicAsync(
                topicName, cancellationToken);
            topicName = rx.Value.Name;
        }

        return cache.Set(topicName, topicName,
            RsbConfiguration.Cache.Expiration);
    }

    private async Task<TopicConfiguration> ConfigureTopicForReceiver(
        MemberInfo messageType, CancellationToken cancellationToken = default)
    {
        var config = new TopicConfiguration(messageType.Name,
            Assembly.GetEntryAssembly()?.GetName().Name);

        var cacheKey = CacheKey(RsbConfiguration.Cache.TopicConfigPrefix,
            config.Name);

        if (cache.TryGetValue(cacheKey, out TopicConfiguration cachedConfig))
            return cachedConfig;

        var admClient = MakeAdmClient();

        if (!await admClient.TopicExistsAsync(config.Name, cancellationToken))
        {
            _ = await admClient.CreateTopicAsync(
                config.Name, cancellationToken);
        }

        if (!await admClient.SubscriptionExistsAsync(config.Name,
                config.SubscriptionName, cancellationToken))
        {
            _ = await admClient
                .CreateSubscriptionAsync(config.Name, config.SubscriptionName,
                    cancellationToken);
        }

        return cache.Set(cacheKey, config, RsbConfiguration.Cache.Expiration);
    }

    //TODO store? throwaway?
    private ServiceBusAdministrationClient MakeAdmClient()
    {
        return new ServiceBusAdministrationClient(RsbConfiguration.ServiceBus
            .ServiceBusConnectionString);
    }

    public ServiceBusSender GetSender(string destination)
    {
        var cacheKey =
            CacheKey(RsbConfiguration.Cache.ServiceBusSenderCachePrefix,
                destination);

        return cache.TryGetValue(cacheKey, out ServiceBusSender sender)
            ? sender
            : cache.Set(cacheKey, _sbClient.CreateSender(destination),
                RsbConfiguration.Cache.Expiration);
    }

    private string CacheKey(params string[] values)
    {
        return string.Join("-", values.Where(x => !string.IsNullOrEmpty(x)));
    }
}

public sealed record TopicConfiguration(string Name, string SubscriptionName);