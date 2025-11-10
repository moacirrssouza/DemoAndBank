using AndBank.Application.DTOs;
using FluentAssertions;
using Xunit;

namespace AndBank.Test.DTOs;

public class PositionDtoTests
{
    [Fact]
    public void PositionDto_ShouldCreateWithRecordSyntax()
    {
        // Arrange
        var positionId = "POS001";
        var productId = "PROD001";
        var clientId = "CLIENT001";
        var date = DateTime.UtcNow;
        var value = 1000.50m;
        var quantity = 10m;

        // Act
        var dto = new PositionDto(positionId, productId, clientId, date, value, quantity);

        // Assert
        dto.Should().NotBeNull();
        dto.PositionId.Should().Be(positionId);
        dto.ProductId.Should().Be(productId);
        dto.ClientId.Should().Be(clientId);
        dto.Date.Should().Be(date);
        dto.Value.Should().Be(value);
        dto.Quantity.Should().Be(quantity);
    }

    [Fact]
    public void PositionDto_ShouldSupportEqualityComparison()
    {
        // Arrange
        var date = DateTime.UtcNow;
        var dto1 = new PositionDto("POS001", "PROD001", "CLIENT001", date, 1000.50m, 10m);
        var dto2 = new PositionDto("POS001", "PROD001", "CLIENT001", date, 1000.50m, 10m);

        // Act & Assert
        // Records support value equality
        dto1.Should().Be(dto2);
        (dto1 == dto2).Should().BeTrue();
    }

    [Fact]
    public void PositionDto_ShouldSupportWithExpression()
    {
        // Arrange
        var originalDate = DateTime.UtcNow;
        var dto = new PositionDto("POS001", "PROD001", "CLIENT001", originalDate, 1000.50m, 10m);

        // Act
        var newDate = DateTime.UtcNow.AddDays(1);
        var updatedDto = dto with { Date = newDate };

        // Assert
        updatedDto.PositionId.Should().Be(dto.PositionId);
        updatedDto.ProductId.Should().Be(dto.ProductId);
        updatedDto.ClientId.Should().Be(dto.ClientId);
        updatedDto.Date.Should().Be(newDate);
        updatedDto.Value.Should().Be(dto.Value);
        updatedDto.Quantity.Should().Be(dto.Quantity);
    }
}


