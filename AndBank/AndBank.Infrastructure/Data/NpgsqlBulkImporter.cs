using AndBank.Domain.Entities;
using Npgsql;
using NpgsqlTypes;

namespace AndBank.Infrastructure.Data;

public class NpgsqlBulkImporter
{
    private readonly string _connectionString;
    public NpgsqlBulkImporter(string connectionString) => _connectionString = connectionString;

    public async Task BulkInsertAsync(IEnumerable<Position> positions, CancellationToken ct = default)
    {
        await using var conn = new NpgsqlConnection(_connectionString);
        await conn.OpenAsync(ct);

        var positionIds = positions.Select(p => p.PositionId).Distinct().ToArray();

        await using (var cmd = new NpgsqlCommand(
            "DELETE FROM positions WHERE position_id = ANY(@ids)", conn))
        {
            cmd.Parameters.AddWithValue("ids", positionIds);
            await cmd.ExecuteNonQueryAsync(ct);
        }

        await using var writer = conn.BeginBinaryImport(
            "COPY positions (position_id, product_id, client_id, date, value, quantity) FROM STDIN (FORMAT BINARY)"
        );

        foreach (var p in positions)
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
    }
}