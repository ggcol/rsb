using Rsb.Configurations;

namespace Playground.Settings;

public class SagaPersistenceSettings : IConfigureSagaPersistence
{
    public string? DataStorageConnectionString { get; set; }
    public string? DataStorageContainer { get; set; }
}