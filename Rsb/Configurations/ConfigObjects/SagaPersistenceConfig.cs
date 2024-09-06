namespace Rsb.Configurations.ConfigObjects;

public sealed class SagaPersistenceConfig : IConfigureSagaPersistence
{
    public string? DataStorageConnectionString { get; set; }
    public string? DataStorageContainer { get; set; }
}