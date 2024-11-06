namespace ASureBus.Abstractions.Configurations;

public interface IConfigureAzureServiceBus
{
    public string? ServiceBusConnectionString { get; set; }
}