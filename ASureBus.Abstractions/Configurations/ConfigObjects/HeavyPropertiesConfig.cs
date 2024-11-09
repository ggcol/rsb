namespace ASureBus.Abstractions.Configurations.ConfigObjects;

public sealed class HeavyPropertiesConfig : IConfigureHeavyProperties
{
    public required string DataStorageConnectionString { get; set; }
    public required string DataStorageContainer { get; set; }
}