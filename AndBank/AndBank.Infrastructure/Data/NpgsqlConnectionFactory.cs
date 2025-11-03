using Microsoft.Extensions.Configuration;
using Npgsql;
using System.Data;

namespace AndBank.Infrastructure.Data;

public class NpgsqlConnectionFactory : IDbConnectionFactory
{
    private readonly IConfiguration _config;

    public NpgsqlConnectionFactory(IConfiguration config)
    {
        _config = config;
    }

    public IDbConnection CreateConnection()
    {
        return new NpgsqlConnection(_config.GetConnectionString("Postgres")!);
    }
}