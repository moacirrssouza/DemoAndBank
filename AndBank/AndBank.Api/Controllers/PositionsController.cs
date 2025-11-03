using AndBank.Domain.Repositories;
using Microsoft.AspNetCore.Mvc;

namespace AndBank.Api.Controllers;

[ApiController]
[Route("api/positions")]
public class PositionsController : ControllerBase
{
    private readonly IPositionRepository _repo;
    public PositionsController(IPositionRepository repo) => _repo = repo;


    [HttpGet("client/{clientId}")]
    public async Task<IActionResult> GetByClient(string clientId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(clientId))
            return BadRequest(new { message = "Client ID is required" });

        var positions = await _repo.GetLatestPositionsByClientAsync(clientId, cancellationToken);

        if (!positions.Any())
            return NotFound(new { message = $"No positions found for client {clientId}" });

        return Ok(positions);
    }


    [HttpGet("client/{clientId}/summary")]
    public async Task<IActionResult> GetSummary(string clientId, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(clientId))
            return BadRequest(new { message = "Client ID is required" });

        var summary = await _repo.GetClientSummaryAsync(clientId, cancellationToken);

        if (!summary.Any())
            return NotFound(new { message = $"No positions found for client {clientId}" });

        return Ok(summary);
    }

    [HttpGet("top10")]
    public async Task<IActionResult> GetTop10(CancellationToken cancellationToken = default)
    {
        var top10 = await _repo.GetTop10ByValueAsync(cancellationToken);

        if (!top10.Any())
            return NotFound(new { message = "No positions found" });

        return Ok(top10);
    }
}