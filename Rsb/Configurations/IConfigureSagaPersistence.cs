namespace Rsb.Configurations;

public interface IConfigureSagaPersistence : IConfigureDataStorage
{
    //TODO add an option to use both in memory and offloaded
}

public sealed class SagaPersistenceConfig : IConfigureSagaPersistence
{
    public string? DataStorageConnectionString { get; set; }
    public string? DataStorageContainer { get; set; }
}