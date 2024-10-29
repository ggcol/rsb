namespace Asb.Configurations;

public interface IConfigureAzureServiceBus
{
    public string? ServiceBusConnectionString { get; set; }
}