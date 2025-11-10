using AndBank.Domain.Entities;
using Npgsql;
using NpgsqlTypes;

namespace AndBank.Infrastructure.Data;

public class NpgsqlBulkImporter
{
    private readonly string _connectionString;

    public NpgsqlBulkImporter(string connectionString) => _connectionString = connectionString;

    public async Task<int> BulkInsertAsync(IEnumerable<Position> positions, CancellationToken ct = default)
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync(ct);

        var positionsList = positions.ToList();
        var totalRecords = positionsList.Count;
        await using var writer = conn.BeginBinaryImport(
            "COPY positions (position_id, product_id, client_id, date, value, quantity) FROM STDIN (FORMAT BINARY)"
        );

        foreach (var p in positionsList)
        {
            await writer.StartRowAsync(ct);
            writer.Write(p.PositionId, NpgsqlDbType.Text);
            writer.Write(p.ProductId, NpgsqlDbType.Text);
            writer.Write(p.ClientId, NpgsqlDbType.Text);
            writer.Write(p.Date, NpgsqlDbType.Timestamp);
            writer.Write(p.Value, NpgsqlDbType.Numeric);
            writer.Write(p.Quantity, NpgsqlDbType.Numeric);
        }

        await writer.CompleteAsync(ct);

        var allPositionIds = positionsList.Select(p => p.PositionId).Distinct().ToArray();
        int importedCount;
        await using (var cmd = new NpgsqlCommand(
            "SELECT COUNT(*) FROM positions WHERE position_id = ANY(@ids)", conn))
        {
            cmd.Parameters.AddWithValue("ids", allPositionIds);
            importedCount = Convert.ToInt32(await cmd.ExecuteScalarAsync(ct));
        }

        if (importedCount != totalRecords)
        {
            throw new InvalidOperationException(
                $"Falha na importação: esperados {totalRecords} registros, mas foram importados {importedCount}");
        }

        return importedCount;
    }
}