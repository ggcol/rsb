using Azure.Messaging.ServiceBus;

namespace Rsb.Configurations.ConfigObjects;

public sealed class ServiceBusConfig : IConfigureAzureServiceBus
{
    // ReSharper disable once InconsistentNaming
    private static readonly ServiceBusClientOptions DEFAULT_CLIENT_OPTIONS = new()
    {
        TransportType = ServiceBusTransportType.AmqpWebSockets
    };

    public string? ServiceBusConnectionString { get; set; }
    public ServiceBusClientOptions ClientOptions { get; set; } = DEFAULT_CLIENT_OPTIONS;
}