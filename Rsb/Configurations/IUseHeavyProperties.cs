namespace Rsb.Configurations;

public interface IUseHeavyProperties
{
    public string? DataStorageConnectionString { get; set; }
    public string? DataStorageContainer { get; set; }
}