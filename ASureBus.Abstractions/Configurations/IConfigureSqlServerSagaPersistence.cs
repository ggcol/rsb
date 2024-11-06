namespace ASureBus.Abstractions.Configurations;

public interface IConfigureSqlServerSagaPersistence
{
    public string? ConnectionString { get; set; }
}