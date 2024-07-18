using System.Reflection;
using Azure.Messaging.ServiceBus;

namespace Rsb.Services;

internal interface IAzureServiceBusService
{
    internal Task<string> ConfigureQueue(string queueName);
    internal Task<string> ConfigureTopicForSender(string topicName);
    internal Task<TopicConfiguration> ConfigureTopicForReceiver(MemberInfo messageType);
    internal ServiceBusSender GetSender(string destination);
    internal ServiceBusProcessor GetProcessor(string queue);
    internal ServiceBusProcessor GetProcessor(string queueOrTopic, string subscriptionName);
}