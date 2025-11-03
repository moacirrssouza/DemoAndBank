using AndBank.Application.DTOs;
using AndBank.Domain.Entities;

namespace AndBank.Domain.Repositories;

public interface IPositionRepository
{
    Task<IEnumerable<Position>> GetLatestPositionsByClientAsync(string clientId, CancellationToken cancellationToken = default);
    Task<IEnumerable<ClientSummaryDto>> GetClientSummaryAsync(string clientId, CancellationToken cancellationToken = default);
    Task<IEnumerable<Position>> GetTop10ByValueAsync(CancellationToken cancellationToken = default);
}