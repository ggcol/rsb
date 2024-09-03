using Azure.Messaging.ServiceBus;
using Rsb.Core.TypesHandling.Entities;

namespace Rsb.Services.ServiceBus;

internal interface IAzureServiceBusService
{
    internal Task<ServiceBusProcessor> GetProcessor(
        ListenerType handler, CancellationToken cancellationToken = default);

    internal Task<string> ConfigureQueue(string queueName,
        CancellationToken cancellationToken = default);

    internal Task<string> ConfigureTopicForSender(string topicName,
        CancellationToken cancellationToken = default);

    internal ServiceBusSender GetSender(string destination);
}