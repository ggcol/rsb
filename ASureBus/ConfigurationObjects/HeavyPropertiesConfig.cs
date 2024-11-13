using ASureBus.Abstractions.Configurations;

namespace ASureBus.ConfigurationObjects;

public sealed class HeavyPropertiesConfig : IConfigureHeavyProperties
{
    public required string DataStorageConnectionString { get; set; }
    public required string DataStorageContainer { get; set; }
}