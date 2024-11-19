using ASureBus.Abstractions.Configurations;

namespace ASureBus.ConfigurationObjects;

public class SqlServerSagaPersistenceConfig : IConfigureSqlServerSagaPersistence
{
    public required string ConnectionString { get; set; }
}