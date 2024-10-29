namespace Asb.Configurations.ConfigObjects;

public class SqlServerSagaPersistenceConfig : IConfigureSqlServerSagaPersistence
{
    public string? ConnectionString { get; set; }
}