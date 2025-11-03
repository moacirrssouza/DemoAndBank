namespace AndBank.Domain.Entities;

public sealed class Position
{
    public string PositionId { get; init; } = default!;
    public string ProductId { get; init; } = default!;
    public string ClientId { get; init; } = default!;
    public DateTime Date { get; init; }
    public decimal Value { get; init; }
    public decimal Quantity { get; init; }
}