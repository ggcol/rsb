using Rsb.Configurations;

namespace Playground.Settings;

public class HeavySettings : IUseHeavyProperties
{
    public string? DataStorageConnectionString { get; set; }
    public string? DataStorageContainer { get; set; }
}