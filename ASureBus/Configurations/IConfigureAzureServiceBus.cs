namespace ASureBus.Configurations;

public interface IConfigureAzureServiceBus
{
    public string? ServiceBusConnectionString { get; set; }
}