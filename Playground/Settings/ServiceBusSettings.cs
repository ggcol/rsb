using Rsb.Configurations;

namespace Playground.Settings;

public class ServiceBusSettings : IConfigureAzureServiceBus
{
    public string? ConnectionString { get; set; }
}