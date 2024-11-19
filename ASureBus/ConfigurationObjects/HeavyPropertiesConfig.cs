using ASureBus.Abstractions.Configurations;

namespace ASureBus.ConfigurationObjects;

public sealed class HeavyPropertiesConfig : IConfigureHeavyProperties
{
    public required string ConnectionString { get; set; }
    public required string Container { get; set; }
}