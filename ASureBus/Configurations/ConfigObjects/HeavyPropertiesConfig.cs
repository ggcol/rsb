namespace ASureBus.Configurations.ConfigObjects;

public sealed class HeavyPropertiesConfig : IConfigureHeavyProperties
{
    public string? DataStorageConnectionString { get; set; }
    public string? DataStorageContainer { get; set; }
}