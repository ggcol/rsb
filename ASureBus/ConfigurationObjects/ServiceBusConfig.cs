using ASureBus.Abstractions.Configurations;
using Azure.Messaging.ServiceBus;

namespace ASureBus.ConfigurationObjects;

public sealed class ServiceBusConfig : IConfigureAzureServiceBus
{
    public required string ConnectionString { get; set; }
    public string? TransportType { get; set; }
    public int? MaxRetries { get; set; }
    public int? DelayInSeconds { get; set; }
    public int? MaxDelayInSeconds { get; set; }
    public int? TryTimeoutInSeconds { get; set; }
    public string? ServiceBusRetryMode { get; set; }
    public int? MaxConcurrentCalls { get; set; }
}

internal sealed class InternalServiceBusConfig
{
    public string ServiceBusConnectionString { get; }
    public ServiceBusClientOptions ClientOptions { get; }
    public int MaxConcurrentCalls { get; }

    public InternalServiceBusConfig(IConfigureAzureServiceBus config)
    {
        ServiceBusConnectionString = config.ConnectionString;
        ClientOptions = new ServiceBusClientOptions()
        {
            TransportType =
                Enum.TryParse<ServiceBusTransportType>(config.TransportType, out var transportType)
                    ? transportType
                    : Defaults.ServiceBus.CLIENT_OPTIONS.TransportType,

            RetryOptions = new ServiceBusRetryOptions()
            {
                Mode = Enum.TryParse<ServiceBusRetryMode>(config.ServiceBusRetryMode,
                    out var retryMode)
                    ? retryMode
                    : Defaults.ServiceBus.CLIENT_OPTIONS.RetryOptions.Mode,

                MaxRetries = config.MaxRetries
                             ?? Defaults.ServiceBus.CLIENT_OPTIONS.RetryOptions.MaxRetries,

                Delay = config.DelayInSeconds is not null
                    ? TimeSpan.FromSeconds(config.DelayInSeconds.Value)
                    : Defaults.ServiceBus.CLIENT_OPTIONS.RetryOptions.Delay,

                MaxDelay = config.MaxDelayInSeconds is not null
                    ? TimeSpan.FromSeconds(config.MaxDelayInSeconds.Value)
                    : Defaults.ServiceBus.CLIENT_OPTIONS.RetryOptions.MaxDelay,

                TryTimeout = config.TryTimeoutInSeconds is not null
                    ? TimeSpan.FromSeconds(config.TryTimeoutInSeconds.Value)
                    : Defaults.ServiceBus.CLIENT_OPTIONS.RetryOptions.TryTimeout
            }
        };
        MaxConcurrentCalls = config.MaxConcurrentCalls ?? Defaults.ServiceBus.MAX_CONCURRENT_CALLS;
    }
}