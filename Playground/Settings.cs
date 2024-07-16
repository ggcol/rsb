using Rsb.Configurations;

namespace Playground;

public class Settings : IConfigureAzureServiceBus
{
    public string? ConnectionString { get; set; }
}