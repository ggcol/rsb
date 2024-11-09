namespace ASureBus.Abstractions.Configurations;

public interface IConfigureDataStorage
{
    public string DataStorageConnectionString { get; set; }
    public string DataStorageContainer { get; set; }
}