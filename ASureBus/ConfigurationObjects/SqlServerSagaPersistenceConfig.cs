using ASureBus.Abstractions.Configurations;

namespace ASureBus.ConfigurationObjects;

public class SqlServerSagaPersistenceConfig : IConfigureSqlServerSagaPersistence
{
    private string? _connectionString;
    
    public required string ConnectionString { get; set; }

    public string? Schema
    {
        get => _connectionString ?? Defaults.SqlServerSagaPersistence.SCHEMA; 
        set => _connectionString = value;
    }
}