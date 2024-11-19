namespace ASureBus.Abstractions.Configurations;

public interface IConfigureDataStorage
{
    public string ConnectionString { get; set; }
    public string Container { get; set; }
}