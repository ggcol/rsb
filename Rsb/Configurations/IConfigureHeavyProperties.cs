namespace Rsb.Configurations;

public interface IConfigureHeavyProperties
{
    public string? DataStorageConnectionString { get; set; }
    public string? DataStorageContainer { get; set; }
}