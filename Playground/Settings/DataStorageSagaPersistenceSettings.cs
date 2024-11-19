using ASureBus.Abstractions.Configurations;

namespace Playground.Settings;

public class DataStorageSagaPersistenceSettings : IConfigureDataStorageSagaPersistence
{
    public string ConnectionString { get; set; } = null!;
    public string Container { get; set; } = null!;
}