using ASureBus.Abstractions.Configurations;

namespace Playground.Settings;

public abstract class ServiceBusSettings : IConfigureAzureServiceBus
{
    public string? ConnectionString { get; set; }
    public string? TransportType { get; set; }
    public int? MaxRetries { get; set; }
    public int? DelayInSeconds { get; set; }
    public int? MaxDelayInSeconds { get; set; }
    public int? TryTimeoutInSeconds { get; set; }
    public string? ServiceBusRetryMode { get; set; }
}

public class WholeServiceBusSettings : ServiceBusSettings { }

public class PartialServiceBusSettings : ServiceBusSettings { }