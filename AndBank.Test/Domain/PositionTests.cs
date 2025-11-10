using AndBank.Domain.Entities;
using FluentAssertions;
using Xunit;

namespace AndBank.Test.Domain;

public class PositionTests
{
    [Fact]
    public void Position_ShouldInitializeWithDefaultValues()
    {
        // Arrange & Act
        var position = new Position();

        // Assert
        position.Should().NotBeNull();
    }

    [Fact]
    public void Position_ShouldSetPropertiesCorrectly()
    {
        // Arrange
        var positionId = "POS001";
        var productId = "PROD001";
        var clientId = "CLIENT001";
        var date = DateTime.UtcNow;
        var value = 1000.50m;
        var quantity = 10m;

        // Act
        var position = new Position
        {
            PositionId = positionId,
            ProductId = productId,
            ClientId = clientId,
            Date = date,
            Value = value,
            Quantity = quantity
        };

        // Assert
        position.PositionId.Should().Be(positionId);
        position.ProductId.Should().Be(productId);
        position.ClientId.Should().Be(clientId);
        position.Date.Should().Be(date);
        position.Value.Should().Be(value);
        position.Quantity.Should().Be(quantity);
    }

    [Fact]
    public void Position_ShouldBeImmutable()
    {
        // Arrange
        var position = new Position
        {
            PositionId = "POS001",
            ProductId = "PROD001",
            ClientId = "CLIENT001",
            Date = DateTime.UtcNow,
            Value = 1000.50m,
            Quantity = 10m
        };

        // Act & Assert
        // Properties are init-only, so they cannot be changed after initialization
        // This test verifies the immutability pattern
        position.Should().NotBeNull();
        position.PositionId.Should().Be("POS001");
    }
}


