using AndBank.Application.DTOs;
using AndBank.Domain.Entities;
using AndBank.Domain.Repositories;
using AndBank.Infrastructure.Data;
using Dapper;
using System.Data;

namespace AndBank.Infrastructure.Repositories;

public class PositionRepository : IPositionRepository
{
    private readonly IDbConnectionFactory _factory;
    public PositionRepository(IDbConnectionFactory factory) => _factory = factory;

    public async Task<IEnumerable<Position>> GetLatestPositionsByClientAsync(string clientId, CancellationToken cancellationToken = default)
    {
        const string sql = @"SELECT DISTINCT ON (position_id)
                                    position_id AS PositionId,
                                    product_id AS ProductId,
                                    client_id AS ClientId,
                                    date AS Date,
                                    value AS Value,
                                    quantity AS Quantity
                               FROM positions
                               WHERE client_id = @ClientId
                               ORDER BY position_id, date DESC;";

        using var conn = _factory.CreateConnection();
        return (await conn.QueryAsync<Position>(sql, new { ClientId = clientId })).ToList();
    }

    public async Task<IEnumerable<ClientSummaryDto>> GetClientSummaryAsync(string clientId, CancellationToken cancellationToken = default)
    {
        const string sql = @"
        SELECT product_id, SUM(value) AS total_value
        FROM (
            SELECT DISTINCT ON (position_id) 
                position_id,
                product_id,
                value
            FROM positions
            WHERE client_id = @ClientId
            ORDER BY position_id, date DESC
        ) latest
        GROUP BY product_id
        ORDER BY total_value DESC";

        using var conn = _factory.CreateConnection();
        var rows = await conn.QueryAsync<dynamic>(sql, new { ClientId = clientId });

        return rows.Select(r => new ClientSummaryDto
        {
            ProductId = (string)r.product_id,
            TotalValue = (decimal)r.total_value
        }).ToList();
    }

    public async Task<IEnumerable<Position>> GetTop10ByValueAsync(CancellationToken cancellationToken = default)
    {
        const string sql = @"SELECT position_id AS PositionId,
                                    product_id AS ProductId,
                                    client_id AS ClientId,
                                    date AS Date,
                                    value AS Value,
                                    quantity AS Quantity
                               FROM positions
                               ORDER BY value DESC
                               LIMIT 10;";

        using var conn = _factory.CreateConnection();
        return (await conn.QueryAsync<Position>(sql)).ToList();
    }
}