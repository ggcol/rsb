using Azure.Messaging.ServiceBus.Administration;

var sbConnectionString = "";
// = "Endpoint=sb://sb-wil-we-dev.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=HNl3w4J0UonlS0EESl/gh47Ngo7N2/0cF+ASbB0srHM=";
var adminClient = new ServiceBusAdministrationClient(sbConnectionString);

// Delete all queues
var queues = adminClient.GetQueuesAsync();
await foreach (var queue in queues)
{
    await adminClient.DeleteQueueAsync(queue.Name);
}

// Delete all topics
var topics = adminClient.GetTopicsAsync();
await foreach (var topic in topics)
{
    await adminClient.DeleteTopicAsync(topic.Name);
}

Console.WriteLine("Done");