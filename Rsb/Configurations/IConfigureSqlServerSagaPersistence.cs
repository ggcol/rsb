namespace Rsb.Configurations;

public interface IConfigureSqlServerSagaPersistence
{
    public string? ConnectionString { get; set; }
}