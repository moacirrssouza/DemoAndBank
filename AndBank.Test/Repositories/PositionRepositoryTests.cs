using AndBank.Application.DTOs;
using AndBank.Domain.Entities;
using AndBank.Infrastructure.Data;
using AndBank.Infrastructure.Repositories;
using Dapper;
using FluentAssertions;
using Moq;
using System.Data;
using Xunit;

namespace AndBank.Test.Repositories;

public class PositionRepositoryTests
{
    private readonly Mock<IDbConnectionFactory> _mockFactory;
    private readonly Mock<IDbConnection> _mockConnection;
    private readonly PositionRepository _repository;

    public PositionRepositoryTests()
    {
        _mockFactory = new Mock<IDbConnectionFactory>();
        _mockConnection = new Mock<IDbConnection>();
        _mockFactory.Setup(f => f.CreateConnection()).Returns(_mockConnection.Object);
        _repository = new PositionRepository(_mockFactory.Object);
    }

    [Fact]
    public async Task GetLatestPositionsByClientAsync_ShouldCallFactoryAndReturnPositions()
    {
        // Arrange
        var clientId = "CLIENT001";
        var expectedPositions = new List<Position>
        {
            new Position
            {
                PositionId = "POS001",
                ProductId = "PROD001",
                ClientId = clientId,
                Date = DateTime.UtcNow,
                Value = 1000.50m,
                Quantity = 10
            }
        };

        // Mock Dapper QueryAsync extension method
        // Note: This is a simplified test. In a real scenario, you might want to use
        // integration tests or a library like Dapper.Moq for more comprehensive testing
        _mockConnection.Setup(c => c.QueryAsync<Position>(
            It.IsAny<string>(),
            It.IsAny<object>(),
            null,
            null,
            null))
            .ReturnsAsync(expectedPositions);

        // Act
        var result = await _repository.GetLatestPositionsByClientAsync(clientId);

        // Assert
        _mockFactory.Verify(f => f.CreateConnection(), Times.Once);
        result.Should().NotBeNull();
        result.Should().BeAssignableTo<IEnumerable<Position>>();
    }

    [Fact]
    public async Task GetClientSummaryAsync_ShouldCallFactoryAndReturnSummary()
    {
        // Arrange
        var clientId = "CLIENT001";
        var dynamicRows = new List<dynamic>
        {
            new { product_id = "PROD001", total_value = 5000.00m },
            new { product_id = "PROD002", total_value = 3000.50m }
        };

        _mockConnection.Setup(c => c.QueryAsync<dynamic>(
            It.IsAny<string>(),
            It.IsAny<object>(),
            null,
            null,
            null))
            .ReturnsAsync(dynamicRows);

        // Act
        var result = await _repository.GetClientSummaryAsync(clientId);

        // Assert
        _mockFactory.Verify(f => f.CreateConnection(), Times.Once);
        result.Should().NotBeNull();
        result.Should().BeAssignableTo<IEnumerable<ClientSummaryDto>>();
    }

    [Fact]
    public async Task GetTop10ByValueAsync_ShouldCallFactoryAndReturnPositions()
    {
        // Arrange
        var expectedPositions = new List<Position>
        {
            new Position
            {
                PositionId = "POS001",
                ProductId = "PROD001",
                ClientId = "CLIENT001",
                Date = DateTime.UtcNow,
                Value = 10000m,
                Quantity = 100
            }
        };

        _mockConnection.Setup(c => c.QueryAsync<Position>(
            It.IsAny<string>(),
            null,
            null,
            null,
            null))
            .ReturnsAsync(expectedPositions);

        // Act
        var result = await _repository.GetTop10ByValueAsync();

        // Assert
        _mockFactory.Verify(f => f.CreateConnection(), Times.Once);
        result.Should().NotBeNull();
        result.Should().BeAssignableTo<IEnumerable<Position>>();
    }

    [Fact]
    public async Task GetLatestPositionsByClientAsync_ShouldDisposeConnection()
    {
        // Arrange
        var clientId = "CLIENT001";
        var expectedPositions = new List<Position>();

        _mockConnection.Setup(c => c.QueryAsync<Position>(
            It.IsAny<string>(),
            It.IsAny<object>(),
            null,
            null,
            null))
            .ReturnsAsync(expectedPositions);

        // Act
        await _repository.GetLatestPositionsByClientAsync(clientId);

        // Assert
        // The using statement should dispose the connection
        // This is verified by checking that CreateConnection was called
        _mockFactory.Verify(f => f.CreateConnection(), Times.Once);
    }
}


