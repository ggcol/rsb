using Asb.Configurations;

namespace Playground.Settings;

public class HeavySettings : IConfigureHeavyProperties
{
    public string? DataStorageConnectionString { get; set; }
    public string? DataStorageContainer { get; set; }
}