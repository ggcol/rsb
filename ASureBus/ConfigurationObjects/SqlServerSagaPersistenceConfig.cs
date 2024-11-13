using ASureBus.Abstractions.Configurations;

namespace ASureBus.ConfigurationObjects;

public class SqlServerSagaPersistenceConfig : IConfigureSqlServerSagaPersistence
{
    public string? ConnectionString { get; set; }
}