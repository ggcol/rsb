namespace Rsb.Configurations;

public interface IConfigureAzureServiceBus
{
    public string? ConnectionString { get; set; }
}