using AndBank.Api.Controllers;
using AndBank.Application.DTOs;
using AndBank.Domain.Entities;
using AndBank.Domain.Repositories;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Xunit;

namespace AndBank.Test.Controllers;

public class PositionsControllerTests
{
    private readonly Mock<IPositionRepository> _mockRepository;
    private readonly PositionsController _controller;

    public PositionsControllerTests()
    {
        _mockRepository = new Mock<IPositionRepository>();
        _controller = new PositionsController(_mockRepository.Object);
    }

    [Fact]
    public async Task GetByClient_WithValidClientId_ReturnsOkWithPositions()
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
            },
            new Position
            {
                PositionId = "POS002",
                ProductId = "PROD002",
                ClientId = clientId,
                Date = DateTime.UtcNow,
                Value = 2000.75m,
                Quantity = 5
            }
        };

        _mockRepository
            .Setup(repo => repo.GetLatestPositionsByClientAsync(clientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedPositions);

        // Act
        var result = await _controller.GetByClient(clientId);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(expectedPositions);
        _mockRepository.Verify(repo => repo.GetLatestPositionsByClientAsync(clientId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetByClient_WithEmptyClientId_ReturnsBadRequest()
    {
        // Arrange
        var clientId = "";

        // Act
        var result = await _controller.GetByClient(clientId);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        var badRequestResult = result as BadRequestObjectResult;
        badRequestResult!.Value.Should().NotBeNull();
        _mockRepository.Verify(repo => repo.GetLatestPositionsByClientAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetByClient_WithWhitespaceClientId_ReturnsBadRequest()
    {
        // Arrange
        var clientId = "   ";

        // Act
        var result = await _controller.GetByClient(clientId);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        _mockRepository.Verify(repo => repo.GetLatestPositionsByClientAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetByClient_WithNullClientId_ReturnsBadRequest()
    {
        // Arrange
        string? clientId = null;

        // Act
        var result = await _controller.GetByClient(clientId!);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        _mockRepository.Verify(repo => repo.GetLatestPositionsByClientAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetByClient_WithNoPositions_ReturnsNotFound()
    {
        // Arrange
        var clientId = "CLIENT999";
        var emptyList = new List<Position>();

        _mockRepository
            .Setup(repo => repo.GetLatestPositionsByClientAsync(clientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(emptyList);

        // Act
        var result = await _controller.GetByClient(clientId);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
        var notFoundResult = result as NotFoundObjectResult;
        notFoundResult!.Value.Should().NotBeNull();
        _mockRepository.Verify(repo => repo.GetLatestPositionsByClientAsync(clientId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetSummary_WithValidClientId_ReturnsOkWithSummary()
    {
        // Arrange
        var clientId = "CLIENT001";
        var expectedSummary = new List<ClientSummaryDto>
        {
            new ClientSummaryDto { ProductId = "PROD001", TotalValue = 5000.00m },
            new ClientSummaryDto { ProductId = "PROD002", TotalValue = 3000.50m }
        };

        _mockRepository
            .Setup(repo => repo.GetClientSummaryAsync(clientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedSummary);

        // Act
        var result = await _controller.GetSummary(clientId);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(expectedSummary);
        _mockRepository.Verify(repo => repo.GetClientSummaryAsync(clientId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetSummary_WithEmptyClientId_ReturnsBadRequest()
    {
        // Arrange
        var clientId = "";

        // Act
        var result = await _controller.GetSummary(clientId);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
        _mockRepository.Verify(repo => repo.GetClientSummaryAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetSummary_WithNoPositions_ReturnsNotFound()
    {
        // Arrange
        var clientId = "CLIENT999";
        var emptyList = new List<ClientSummaryDto>();

        _mockRepository
            .Setup(repo => repo.GetClientSummaryAsync(clientId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(emptyList);

        // Act
        var result = await _controller.GetSummary(clientId);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
        _mockRepository.Verify(repo => repo.GetClientSummaryAsync(clientId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetTop10_WithPositions_ReturnsOkWithTop10()
    {
        // Arrange
        var expectedTop10 = new List<Position>
        {
            new Position { PositionId = "POS001", ProductId = "PROD001", ClientId = "CLIENT001", Date = DateTime.UtcNow, Value = 10000m, Quantity = 100 },
            new Position { PositionId = "POS002", ProductId = "PROD002", ClientId = "CLIENT002", Date = DateTime.UtcNow, Value = 9000m, Quantity = 90 },
            new Position { PositionId = "POS003", ProductId = "PROD003", ClientId = "CLIENT003", Date = DateTime.UtcNow, Value = 8000m, Quantity = 80 }
        };

        _mockRepository
            .Setup(repo => repo.GetTop10ByValueAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(expectedTop10);

        // Act
        var result = await _controller.GetTop10();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.Value.Should().BeEquivalentTo(expectedTop10);
        _mockRepository.Verify(repo => repo.GetTop10ByValueAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetTop10_WithNoPositions_ReturnsNotFound()
    {
        // Arrange
        var emptyList = new List<Position>();

        _mockRepository
            .Setup(repo => repo.GetTop10ByValueAsync(It.IsAny<CancellationToken>()))
            .ReturnsAsync(emptyList);

        // Act
        var result = await _controller.GetTop10();

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
        var notFoundResult = result as NotFoundObjectResult;
        notFoundResult!.Value.Should().NotBeNull();
        _mockRepository.Verify(repo => repo.GetTop10ByValueAsync(It.IsAny<CancellationToken>()), Times.Once);
    }
}


