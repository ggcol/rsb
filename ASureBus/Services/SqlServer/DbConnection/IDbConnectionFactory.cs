namespace ASureBus.Services.SqlServer.DbConnection;

public interface IDbConnectionFactory
{
    System.Data.Common.DbConnection CreateConnection();
}