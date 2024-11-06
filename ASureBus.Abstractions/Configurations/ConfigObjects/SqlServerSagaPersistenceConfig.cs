namespace ASureBus.Abstractions.Configurations.ConfigObjects;

public class SqlServerSagaPersistenceConfig : IConfigureSqlServerSagaPersistence
{
    public string? ConnectionString { get; set; }
}