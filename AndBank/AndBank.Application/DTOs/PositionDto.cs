namespace AndBank.Application.DTOs;

public record PositionDto(string PositionId, string ProductId, string ClientId, DateTime Date, decimal Value, decimal Quantity);