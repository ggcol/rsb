namespace Asb.Configurations;

public interface IConfigureDataStorage
{
    public string? DataStorageConnectionString { get; set; }
    public string? DataStorageContainer { get; set; }
}