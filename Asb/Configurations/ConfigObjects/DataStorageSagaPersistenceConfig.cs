namespace Asb.Configurations.ConfigObjects;

public sealed class DataStorageSagaPersistenceConfig : IConfigureDataStorageSagaPersistence
{
    public string? DataStorageConnectionString { get; set; }
    public string? DataStorageContainer { get; set; }
}