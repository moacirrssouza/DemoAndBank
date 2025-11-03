using System.Data;

namespace AndBank.Infrastructure.Data;

public interface IDbConnectionFactory
{
    IDbConnection CreateConnection();
}