using ASureBus.Abstractions.Configurations;

namespace Playground.Settings;

public class DataStorageSagaPersistenceSettings : IConfigureDataStorageSagaPersistence
{
    public string? DataStorageConnectionString { get; set; }
    public string? DataStorageContainer { get; set; }
}