using Microsoft.Data.SqlClient;
using ASureBus.Core;
using ASureBus.Core.Sagas;
using ASureBus.Core.TypesHandling.Entities;
using ASureBus.Utils;

namespace ASureBus.Services.SqlServer;

internal sealed class SqlServerService : ISqlServerService
{
    private const string CORR_ID_HEADER = "CorrelationId";
    private const string SAGA_HEADER = "Saga";
    private const string CORR_ID_PARAM = "@" + CORR_ID_HEADER;
    private const string SAGA_PARAM = "@" + SAGA_HEADER;
    private const string TABLE_NAME = "@tableName";

    private readonly string _connectionString =
        AsbConfiguration.SqlServerSagaPersistence?.ConnectionString!;

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

        const string updateQuery = $"""
                                    UPDATE {TABLE_NAME}
                                    SET {SAGA_HEADER} = {SAGA_PARAM} 
                                    WHERE {CORR_ID_HEADER} = {CORR_ID_PARAM}
                                    """;

        await using var conn = new SqlConnection(_connectionString);

        var cmd = new SqlCommand(updateQuery, conn);
        cmd.Parameters.AddWithValue(TABLE_NAME, tableName);
        cmd.Parameters.AddWithValue(CORR_ID_PARAM, correlationId);
        cmd.Parameters.AddWithValue(SAGA_PARAM, serialized);

        await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

        var result = await cmd.ExecuteNonQueryAsync(cancellationToken);

        if (result == 1) return;

        const string query = $"""
                              INSERT INTO {TABLE_NAME} ({CORR_ID_HEADER}, {SAGA_HEADER}) 
                                VALUES ({CORR_ID_PARAM}, {SAGA_PARAM})
                              """;

        cmd = new SqlCommand(query, conn);
        cmd.Parameters.AddWithValue(TABLE_NAME, tableName);
        cmd.Parameters.AddWithValue(CORR_ID_PARAM, correlationId);
        cmd.Parameters.AddWithValue(SAGA_PARAM, serialized);

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

        if (!await TableExists(tableName, cancellationToken).ConfigureAwait(false))
        {
            //TODO customize
            throw new Exception();
        }

        var query = $"""
                     SELECT {SAGA_HEADER}
                     FROM {TABLE_NAME}
                     WHERE {CORR_ID_HEADER} = {CORR_ID_PARAM}
                     """;

        await using var conn = new SqlConnection(_connectionString);

        var cmd = new SqlCommand(query, conn);
        cmd.Parameters.AddWithValue(CORR_ID_PARAM, correlationId);
        cmd.Parameters.AddWithValue(TABLE_NAME, tableName);

        await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

        var reader = await cmd.ExecuteReaderAsync(cancellationToken).ConfigureAwait(false);

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

        if (!await TableExists(tableName, cancellationToken).ConfigureAwait(false))
        {
            //TODO customize
            throw new Exception();
        }

        const string query = $"""
                              DELETE
                              FROM {TABLE_NAME}
                              WHERE {CORR_ID_HEADER} = {CORR_ID_PARAM}
                              """;

        await using var conn = new SqlConnection(_connectionString);
        var cmd = new SqlCommand(query, conn);
        cmd.Parameters.AddWithValue(CORR_ID_PARAM, correlationId);
        cmd.Parameters.AddWithValue(TABLE_NAME, tableName);

        await conn.OpenAsync(cancellationToken).ConfigureAwait(false);

        var result = await cmd.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);

        if (result != 1)
        {
            //TODO customize
            throw new Exception();
        }
    }

    private async Task<bool> TableExists(string tableName, CancellationToken cancellationToken)
    {
        const string checkTableQuery =
            $"IF OBJECT_ID({TABLE_NAME}, 'U') IS NOT NULL SELECT 1 ELSE SELECT 0";

        await using var connection = new SqlConnection(_connectionString);

        var checkCommand = new SqlCommand(checkTableQuery, connection);

        checkCommand.Parameters.AddWithValue(TABLE_NAME, tableName);
        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);

        return (int)await checkCommand
            .ExecuteScalarAsync(cancellationToken)
            .ConfigureAwait(false) == 1;
    }

    private async Task CreateTable(string tableName, CancellationToken cancellationToken)
    {
        const string createTableQuery = $"""
                                         CREATE TABLE {TABLE_NAME} (
                                             {CORR_ID_HEADER} UNIQUEIDENTIFIER PRIMARY KEY,
                                             {SAGA_HEADER} NVARCHAR(MAX)
                                         )
                                         """;

        await using var connection = new SqlConnection(_connectionString);

        var createCommand = new SqlCommand(createTableQuery, connection);
        createCommand.Parameters.AddWithValue(TABLE_NAME, tableName);

        await connection.OpenAsync(cancellationToken).ConfigureAwait(false);
        await createCommand.ExecuteNonQueryAsync(cancellationToken).ConfigureAwait(false);
    }
}