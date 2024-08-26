using System.Reflection;
using Azure.Messaging.ServiceBus;

namespace Rsb.Services;

internal interface IAzureServiceBusService
{
    internal Task<string> ConfigureQueue(string queueName, 
        CancellationToken cancellationToken = default);
    internal Task<string> ConfigureTopicForSender(string topicName, 
        CancellationToken cancellationToken = default);
    internal Task<TopicConfiguration> ConfigureTopicForReceiver(
        MemberInfo messageType, CancellationToken cancellationToken = default);
    internal ServiceBusSender GetSender(string destination);
    internal ServiceBusProcessor GetProcessor(string queue);
    internal ServiceBusProcessor GetProcessor(string queueOrTopic, 
        string subscriptionName);
}