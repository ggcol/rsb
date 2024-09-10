namespace Rsb.Configurations;

public class SqlServerSagaPersistenceConfig : IConfigureSqlServerSagaPersistence
{
    public string? ConnectionString { get; set; }
}