using Microsoft.Data.SqlClient;
using Rsb.Core;
using Rsb.Core.Sagas;
using Rsb.Core.TypesHandling.Entities;
using Rsb.Services.StorageAccount;
using Rsb.Utils;

namespace Rsb.Services.SqlServer;

internal sealed class SqlServerService : ISqlServerService
{
    private const string CORR_ID_HEADER = "CorrelationId";
    private const string SAGA_HEADER = "Saga";
    private const string CORR_ID_PARAM = "@" + CORR_ID_HEADER;
    private const string SAGA_PARAM = "@" + SAGA_HEADER;

    private readonly string CONNECTION_STRING =
        RsbConfiguration.SqlServerSagaPersistence?.ConnectionString!;

    public async Task Save<TItem>(TItem item, SagaType sagaType,
        Guid correlationId, CancellationToken cancellationToken = default)
    {
        var tableName = sagaType.Type.Name;

        if (!await TableExists(tableName, cancellationToken)
                .ConfigureAwait(false))
        {
            await CreateTable(tableName, cancellationToken)
                .ConfigureAwait(false);
        }

        var serialized = Serializer.Serialize(item);

        var query = $@"UPDATE {tableName} 
                    SET {SAGA_HEADER} = {SAGA_PARAM} 
                    WHERE {CORR_ID_HEADER} = {CORR_ID_PARAM}";
        await using var conn = new SqlConnection(CONNECTION_STRING);

        var cmd = new SqlCommand(query, conn);
        cmd.Parameters.AddWithValue(CORR_ID_PARAM, correlationId);
        cmd.Parameters.AddWithValue(SAGA_PARAM, serialized);

        await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

        var result = await cmd.ExecuteNonQueryAsync(cancellationToken);

        if (result == 1)
        {
            return;
        }

        query = @$"INSERT INTO {tableName} 
                    ({CORR_ID_HEADER}, {SAGA_HEADER}) 
                VALUES ({CORR_ID_PARAM}, {SAGA_PARAM})";

        cmd = new SqlCommand(query, conn);
        cmd.Parameters.AddWithValue(CORR_ID_PARAM, correlationId);
        cmd.Parameters.AddWithValue(SAGA_PARAM, serialized);

        // await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

        result = await cmd.ExecuteNonQueryAsync(cancellationToken);

        if (result != 1)
        {
            //TODO customize
            throw new Exception();
        }
    }

    public async Task<object?> Get(SagaType sagaType, Guid correlationId,
        CancellationToken cancellationToken = default)
    {
        var tableName = sagaType.Type.Name;

        if (!await TableExists(tableName, cancellationToken)
                .ConfigureAwait(false))
        {
            //TODO customize
            throw new Exception();
        }

        var query = $@"SELECT {SAGA_HEADER} 
                    FROM {tableName} 
                    WHERE {CORR_ID_HEADER} = {CORR_ID_PARAM}";
        await using var conn = new SqlConnection(CONNECTION_STRING);

        var cmd = new SqlCommand(query, conn);
        cmd.Parameters.AddWithValue(CORR_ID_PARAM, correlationId);

        await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

        var reader = await cmd.ExecuteReaderAsync(cancellationToken)
            .ConfigureAwait(false);

        if (!reader.HasRows)
        {
            //TODO customize
            throw new Exception();
        }

        await reader.ReadAsync(cancellationToken).ConfigureAwait(false);
        var result = reader.GetString(0);

        return Serializer.Deserialize(result, sagaType.Type,
            new SagaConverter(sagaType.Type, sagaType.SagaDataType));
    }

    public async Task Delete(SagaType sagaType, Guid correlationId,
        CancellationToken cancellationToken = default)
    {
        var tableName = sagaType.Type.Name;

        if (!await TableExists(tableName, cancellationToken)
                .ConfigureAwait(false))
        {
            //TODO customize
            throw new Exception();
        }

        var query = @$"DELETE 
                    FROM {tableName} 
                    WHERE {CORR_ID_HEADER} = {CORR_ID_PARAM}";
        await using var conn = new SqlConnection(CONNECTION_STRING);

        var cmd = new SqlCommand(query, conn);
        cmd.Parameters.AddWithValue(CORR_ID_PARAM, correlationId);

        await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

        var result = await cmd.ExecuteNonQueryAsync(cancellationToken)
            .ConfigureAwait(false);

        if (result != 1)
        {
            //TODO customize
            throw new Exception();
        }
    }

    private async Task<bool> TableExists(string tableName,
        CancellationToken cancellationToken)
    {
        var checkTableQuery = $@"IF OBJECT_ID('{tableName}', 'U') 
                              IS NOT NULL SELECT 1 ELSE SELECT 0";

        await using var connection = new SqlConnection(CONNECTION_STRING);
        var checkCommand = new SqlCommand(checkTableQuery, connection);
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

        return (int)await checkCommand
            .ExecuteScalarAsync(cancellationToken) == 1;
    }

    private async Task CreateTable(string tableName,
        CancellationToken cancellationToken)
    {
        var createTableQuery = @$"CREATE TABLE {tableName} (
                {CORR_ID_HEADER} UNIQUEIDENTIFIER PRIMARY KEY,
                {SAGA_HEADER} NVARCHAR(MAX))";

        await using var connection = new SqlConnection(CONNECTION_STRING);
        var createCommand = new SqlCommand(createTableQuery, connection);
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
        await createCommand.ExecuteNonQueryAsync(cancellationToken);
    }
}