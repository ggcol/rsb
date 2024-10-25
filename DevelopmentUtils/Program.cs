using Azure.Messaging.ServiceBus.Administration;

var sbConnectionString 
    = "Endpoint=sb://sb-wil-we-dev.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=HNl3w4J0UonlS0EESl/gh47Ngo7N2/0cF+ASbB0srHM=";

var adminClient = new ServiceBusAdministrationClient(sbConnectionString);

var queues = adminClient.GetQueuesAsync();
await foreach (var queue in queues)
{
    if (queue.Name.Contains("Rebus", StringComparison.OrdinalIgnoreCase))
        continue;
    
    await adminClient.DeleteQueueAsync(queue.Name);
}

var topics = adminClient.GetTopicsAsync();
await foreach (var topic in topics)
{
    if (topic.Name.Contains("Rebus", StringComparison.OrdinalIgnoreCase))
        continue;
    await adminClient.DeleteTopicAsync(topic.Name);
}

Console.WriteLine("Done");