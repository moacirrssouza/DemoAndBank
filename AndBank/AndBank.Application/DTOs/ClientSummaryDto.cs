namespace AndBank.Application.DTOs;

public record ClientSummaryDto
{
    public string ProductId { get; init; }
    public decimal TotalValue { get; init; }
}