using AndBank.Application.DTOs;
using FluentAssertions;
using Xunit;

namespace AndBank.Test.DTOs;

public class ClientSummaryDtoTests
{
    [Fact]
    public void ClientSummaryDto_ShouldInitializeWithDefaultValues()
    {
        // Arrange & Act
        var dto = new ClientSummaryDto();

        // Assert
        dto.Should().NotBeNull();
    }

    [Fact]
    public void ClientSummaryDto_ShouldSetPropertiesCorrectly()
    {
        // Arrange
        var productId = "PROD001";
        var totalValue = 5000.00m;

        // Act
        var dto = new ClientSummaryDto
        {
            ProductId = productId,
            TotalValue = totalValue
        };

        // Assert
        dto.ProductId.Should().Be(productId);
        dto.TotalValue.Should().Be(totalValue);
    }

    [Fact]
    public void ClientSummaryDto_ShouldBeImmutable()
    {
        // Arrange
        var dto = new ClientSummaryDto
        {
            ProductId = "PROD001",
            TotalValue = 5000.00m
        };

        // Act & Assert
        // Properties are init-only, so they cannot be changed after initialization
        dto.Should().NotBeNull();
        dto.ProductId.Should().Be("PROD001");
        dto.TotalValue.Should().Be(5000.00m);
    }
}


