using Asb.Configurations;

namespace Playground.Settings;

public class SqlServerSagaPersistenceSettings : IConfigureSqlServerSagaPersistence
{
    public string? ConnectionString { get; set; }
}