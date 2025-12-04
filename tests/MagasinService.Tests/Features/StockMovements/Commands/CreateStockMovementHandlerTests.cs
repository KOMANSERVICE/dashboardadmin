using FluentAssertions;
using IDR.Library.BuildingBlocks.Exceptions;
using MagasinService.Application.Commons.Interfaces;
using MagasinService.Application.Features.StockMovements.Commands.CreateStockMovement;
using MagasinService.Application.Features.StockMovements.DTOs;
using MagasinService.Domain.Entities;
using MagasinService.Domain.Enums;
using MagasinService.Domain.ValueObjects;
using Moq;
using Xunit;

namespace MagasinService.Tests.Features.StockMovements.Commands;

public class CreateStockMovementHandlerTests
{
    private readonly Mock<IStockMovementRepository> _mockStockMovementRepository;
    private readonly Mock<IStockLocationRepository> _mockStockLocationRepository;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly CreateStockMovementHandler _handler;

    public CreateStockMovementHandlerTests()
    {
        _mockStockMovementRepository = new Mock<IStockMovementRepository>();
        _mockStockLocationRepository = new Mock<IStockLocationRepository>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();

        _handler = new CreateStockMovementHandler(
            _mockStockMovementRepository.Object,
            _mockStockLocationRepository.Object,
            _mockUnitOfWork.Object);
    }

    [Fact]
    public async Task Handle_ValidRequest_CreatesStockMovement()
    {
        // Arrange
        var boutiqueId = Guid.NewGuid();
        var sourceLocationId = Guid.NewGuid();
        var destinationLocationId = Guid.NewGuid();
        var productId = Guid.NewGuid();

        var sourceLocation = new StockLocation
        {
            Id = StockLocationId.Of(sourceLocationId),
            Name = "Magasin A",
            BoutiqueId = boutiqueId,
            Type = StockLocationType.Store
        };

        var destinationLocation = new StockLocation
        {
            Id = StockLocationId.Of(destinationLocationId),
            Name = "Magasin B",
            BoutiqueId = boutiqueId,
            Type = StockLocationType.Store
        };

        var request = new CreateStockMovementRequest
        {
            Quantity = 10,
            Reference = "MOV-2024-001",
            MovementType = StockMovementType.Transfer,
            ProductId = productId,
            SourceLocationId = sourceLocationId,
            DestinationLocationId = destinationLocationId
        };

        var command = new CreateStockMovementCommand(request);

        _mockStockLocationRepository
            .Setup(x => x.GetByIdAsync(sourceLocationId))
            .ReturnsAsync(sourceLocation);

        _mockStockLocationRepository
            .Setup(x => x.GetByIdAsync(destinationLocationId))
            .ReturnsAsync(destinationLocation);

        _mockStockMovementRepository
            .Setup(x => x.AddAsync(It.IsAny<StockMovement>()))
            .Returns(Task.CompletedTask);

        _mockStockMovementRepository
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync((Guid id) => new StockMovement
            {
                Id = StockMovementId.Of(id),
                Quantity = request.Quantity,
                Reference = request.Reference,
                MovementType = request.MovementType,
                ProductId = request.ProductId,
                SourceLocationId = StockLocationId.Of(sourceLocationId),
                DestinationLocationId = StockLocationId.Of(destinationLocationId),
                Date = DateTime.UtcNow,
                SourceLocation = sourceLocation,
                DestinationLocation = destinationLocation
            });

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Quantity.Should().Be(request.Quantity);
        result.Reference.Should().Be(request.Reference);
        result.MovementType.Should().Be(request.MovementType);
        result.ProductId.Should().Be(request.ProductId);
        result.SourceLocationId.Should().Be(sourceLocationId);
        result.DestinationLocationId.Should().Be(destinationLocationId);

        _mockStockMovementRepository.Verify(x => x.AddAsync(It.IsAny<StockMovement>()), Times.Once);
        _mockUnitOfWork.Verify(x => x.SaveChangesDataAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_SourceLocationNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var request = new CreateStockMovementRequest
        {
            Quantity = 10,
            Reference = "MOV-2024-001",
            MovementType = StockMovementType.Transfer,
            ProductId = Guid.NewGuid(),
            SourceLocationId = Guid.NewGuid(),
            DestinationLocationId = Guid.NewGuid()
        };

        var command = new CreateStockMovementCommand(request);

        _mockStockLocationRepository
            .Setup(x => x.GetByIdAsync(request.SourceLocationId))
            .ReturnsAsync((StockLocation?)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() =>
            _handler.Handle(command, CancellationToken.None));
    }

    [Fact]
    public async Task Handle_DifferentBoutiques_ThrowsDomainException()
    {
        // Arrange
        var sourceLocationId = Guid.NewGuid();
        var destinationLocationId = Guid.NewGuid();

        var sourceLocation = new StockLocation
        {
            Id = StockLocationId.Of(sourceLocationId),
            Name = "Magasin A",
            BoutiqueId = Guid.NewGuid(),
            Type = StockLocationType.Store
        };

        var destinationLocation = new StockLocation
        {
            Id = StockLocationId.Of(destinationLocationId),
            Name = "Magasin B",
            BoutiqueId = Guid.NewGuid(), // Boutique différente
            Type = StockLocationType.Store
        };

        var request = new CreateStockMovementRequest
        {
            Quantity = 10,
            Reference = "MOV-2024-001",
            MovementType = StockMovementType.Transfer,
            ProductId = Guid.NewGuid(),
            SourceLocationId = sourceLocationId,
            DestinationLocationId = destinationLocationId
        };

        var command = new CreateStockMovementCommand(request);

        _mockStockLocationRepository
            .Setup(x => x.GetByIdAsync(sourceLocationId))
            .ReturnsAsync(sourceLocation);

        _mockStockLocationRepository
            .Setup(x => x.GetByIdAsync(destinationLocationId))
            .ReturnsAsync(destinationLocation);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<DomainException>(() =>
            _handler.Handle(command, CancellationToken.None));

        exception.Message.Should().Contain("même boutique");
    }

    [Fact]
    public async Task Handle_SameSourceAndDestination_ThrowsDomainException()
    {
        // Arrange
        var locationId = Guid.NewGuid();
        var boutiqueId = Guid.NewGuid();

        var location = new StockLocation
        {
            Id = StockLocationId.Of(locationId),
            Name = "Magasin A",
            BoutiqueId = boutiqueId,
            Type = StockLocationType.Store
        };

        var request = new CreateStockMovementRequest
        {
            Quantity = 10,
            Reference = "MOV-2024-001",
            MovementType = StockMovementType.Transfer,
            ProductId = Guid.NewGuid(),
            SourceLocationId = locationId,
            DestinationLocationId = locationId // Même location
        };

        var command = new CreateStockMovementCommand(request);

        _mockStockLocationRepository
            .Setup(x => x.GetByIdAsync(locationId))
            .ReturnsAsync(location);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<DomainException>(() =>
            _handler.Handle(command, CancellationToken.None));

        exception.Message.Should().Contain("identiques");
    }
}