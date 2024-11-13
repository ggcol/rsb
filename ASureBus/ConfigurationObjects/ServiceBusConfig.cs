using ASureBus.Abstractions.Configurations;
using Azure.Messaging.ServiceBus;

namespace ASureBus.ConfigurationObjects;

public sealed class ServiceBusConfig : IConfigureAzureServiceBus
{
    private ServiceBusClientOptions? _clientOptions;

    public string? ServiceBusConnectionString { get; set; }
    public ServiceBusClientOptions? ClientOptions
    {
        get => _clientOptions ?? Defaults.ServiceBus.CLIENT_OPTIONS; 
        set => _clientOptions = value;
    }
}