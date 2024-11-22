using Microsoft.Data.SqlClient;

namespace ASureBus.Services.SqlServer.DbConnection;

public class SqlServerConnectionFactory(string connectionString) : IDbConnectionFactory
{
    public System.Data.Common.DbConnection CreateConnection()
    {
        return new SqlConnection(connectionString);
    }
}