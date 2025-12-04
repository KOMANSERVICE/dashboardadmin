using FluentAssertions;
using Moq;
using MagasinService.Application.Commons.Interfaces;
using MagasinService.Application.Features.StockMovements.Commands.CreateStockMovement;
using MagasinService.Application.Features.StockMovements.DTOs;
using MagasinService.Domain.Entities;
using MagasinService.Domain.Enums;
using MagasinService.Domain.Exceptions;
using MagasinService.Domain.ValueObjects;
using IDR.Library.BuildingBlocks.Exceptions;
using Mapster;

namespace MagasinService.Tests.Features.StockMovements;

public class CreateStockMovementHandlerTests
{
    private readonly Mock<IStockMovementRepository> _stockMovementRepositoryMock;
    private readonly Mock<IStockLocationRepository> _stockLocationRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly CreateStockMovementHandler _handler;
    private readonly List<StockLocation> _testLocations;

    public CreateStockMovementHandlerTests()
    {
        _stockMovementRepositoryMock = new Mock<IStockMovementRepository>();
        _stockLocationRepositoryMock = new Mock<IStockLocationRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        _handler = new CreateStockMovementHandler(
            _stockMovementRepositoryMock.Object,
            _stockLocationRepositoryMock.Object,
            _unitOfWorkMock.Object
        );

        // Setup test data
        _testLocations = SetupTestData();
    }

    private List<StockLocation> SetupTestData()
    {
        var boutiqueId = Guid.NewGuid();
        var locations = new List<StockLocation>
        {
            new StockLocation
            {
                Id = StockLocationId.Of(Guid.NewGuid()),
                Name = "Magasin Principal",
                Address = "123 Rue Principale",
                Type = StockLocationType.Store,
                BoutiqueId = boutiqueId
            },
            new StockLocation
            {
                Id = StockLocationId.Of(Guid.NewGuid()),
                Name = "Magasin Secondaire",
                Address = "456 Rue Secondaire",
                Type = StockLocationType.Store,
                BoutiqueId = boutiqueId
            },
            new StockLocation
            {
                Id = StockLocationId.Of(Guid.NewGuid()),
                Name = "Entrepôt",
                Address = "789 Zone Industrielle",
                Type = StockLocationType.Warehouse,
                BoutiqueId = Guid.NewGuid() // Different boutique
            }
        };

        return locations;
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldCreateStockMovement()
    {
        // Arrange
        var request = new CreateStockMovementRequest
        {
            Quantity = 10,
            Reference = "MVT-TEST-001",
            MovementType = StockMovementType.Transfer,
            ProductId = Guid.NewGuid(),
            SourceLocationId = _testLocations[0].Id.Value,
            DestinationLocationId = _testLocations[1].Id.Value
        };

        var command = new CreateStockMovementCommand(request);

        // Setup mocks
        _stockLocationRepositoryMock
            .Setup(x => x.GetByIdAsync(request.SourceLocationId))
            .ReturnsAsync(_testLocations[0]);

        _stockLocationRepositoryMock
            .Setup(x => x.GetByIdAsync(request.DestinationLocationId))
            .ReturnsAsync(_testLocations[1]);

        var savedMovement = new StockMovement
        {
            Id = StockMovementId.Of(Guid.NewGuid()),
            Quantity = request.Quantity,
            Reference = request.Reference,
            MovementType = request.MovementType,
            ProductId = request.ProductId,
            SourceLocationId = _testLocations[0].Id,
            DestinationLocationId = _testLocations[1].Id,
            SourceLocation = _testLocations[0],
            DestinationLocation = _testLocations[1],
            Date = DateTime.UtcNow
        };

        _stockMovementRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<StockMovement>()))
            .ReturnsAsync((StockMovement m) => {
                savedMovement = m;
                savedMovement.SourceLocation = _testLocations[0];
                savedMovement.DestinationLocation = _testLocations[1];
                return savedMovement;
            });

        _stockMovementRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(savedMovement);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().NotBeEmpty();
        result.Quantity.Should().Be(request.Quantity);
        result.Reference.Should().Be(request.Reference);
        result.MovementType.Should().Be(request.MovementType);
        result.ProductId.Should().Be(request.ProductId);
        result.SourceLocationId.Should().Be(request.SourceLocationId);
        result.DestinationLocationId.Should().Be(request.DestinationLocationId);

        // Verify mocks
        _stockLocationRepositoryMock.Verify(x => x.GetByIdAsync(request.SourceLocationId), Times.Once);
        _stockLocationRepositoryMock.Verify(x => x.GetByIdAsync(request.DestinationLocationId), Times.Once);
        _stockMovementRepositoryMock.Verify(x => x.AddAsync(It.IsAny<StockMovement>()), Times.Once);
        _unitOfWorkMock.Verify(x => x.SaveChangesDataAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_DifferentBoutiques_ShouldThrowDomainException()
    {
        // Arrange
        var request = new CreateStockMovementRequest
        {
            Quantity = 10,
            Reference = "MVT-TEST-002",
            MovementType = StockMovementType.Transfer,
            ProductId = Guid.NewGuid(),
            SourceLocationId = _testLocations[0].Id.Value, // Boutique 1
            DestinationLocationId = _testLocations[2].Id.Value // Different boutique
        };

        var command = new CreateStockMovementCommand(request);

        // Setup mocks
        _stockLocationRepositoryMock
            .Setup(x => x.GetByIdAsync(request.SourceLocationId))
            .ReturnsAsync(_testLocations[0]);

        _stockLocationRepositoryMock
            .Setup(x => x.GetByIdAsync(request.DestinationLocationId))
            .ReturnsAsync(_testLocations[2]);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*même boutique*");

        // Verify mocks
        _stockMovementRepositoryMock.Verify(x => x.AddAsync(It.IsAny<StockMovement>()), Times.Never);
        _unitOfWorkMock.Verify(x => x.SaveChangesDataAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_NonExistentSourceLocation_ShouldThrowNotFoundException()
    {
        // Arrange
        var request = new CreateStockMovementRequest
        {
            Quantity = 10,
            Reference = "MVT-TEST-003",
            MovementType = StockMovementType.Transfer,
            ProductId = Guid.NewGuid(),
            SourceLocationId = Guid.NewGuid(), // Non-existent
            DestinationLocationId = _testLocations[1].Id.Value
        };

        var command = new CreateStockMovementCommand(request);

        // Setup mocks
        _stockLocationRepositoryMock
            .Setup(x => x.GetByIdAsync(request.SourceLocationId))
            .ReturnsAsync((StockLocation?)null);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("*StockLocation*");

        // Verify mocks
        _stockLocationRepositoryMock.Verify(x => x.GetByIdAsync(request.DestinationLocationId), Times.Never);
        _stockMovementRepositoryMock.Verify(x => x.AddAsync(It.IsAny<StockMovement>()), Times.Never);
        _unitOfWorkMock.Verify(x => x.SaveChangesDataAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_NonExistentDestinationLocation_ShouldThrowNotFoundException()
    {
        // Arrange
        var request = new CreateStockMovementRequest
        {
            Quantity = 10,
            Reference = "MVT-TEST-003b",
            MovementType = StockMovementType.Transfer,
            ProductId = Guid.NewGuid(),
            SourceLocationId = _testLocations[0].Id.Value,
            DestinationLocationId = Guid.NewGuid() // Non-existent
        };

        var command = new CreateStockMovementCommand(request);

        // Setup mocks
        _stockLocationRepositoryMock
            .Setup(x => x.GetByIdAsync(request.SourceLocationId))
            .ReturnsAsync(_testLocations[0]);

        _stockLocationRepositoryMock
            .Setup(x => x.GetByIdAsync(request.DestinationLocationId))
            .ReturnsAsync((StockLocation?)null);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("*StockLocation*");

        // Verify mocks
        _stockMovementRepositoryMock.Verify(x => x.AddAsync(It.IsAny<StockMovement>()), Times.Never);
        _unitOfWorkMock.Verify(x => x.SaveChangesDataAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_SameSourceAndDestination_ShouldThrowDomainException()
    {
        // Arrange
        var locationId = _testLocations[0].Id.Value;
        var request = new CreateStockMovementRequest
        {
            Quantity = 10,
            Reference = "MVT-TEST-004",
            MovementType = StockMovementType.Transfer,
            ProductId = Guid.NewGuid(),
            SourceLocationId = locationId,
            DestinationLocationId = locationId // Same as source
        };

        var command = new CreateStockMovementCommand(request);

        // Setup mocks - not needed as the validation occurs before repository calls

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*source et destination ne peuvent pas être identiques*");
    }

    [Fact]
    public async Task Handle_ZeroQuantity_ShouldCreateMovement()
    {
        // Arrange
        var request = new CreateStockMovementRequest
        {
            Quantity = 0,
            Reference = "MVT-TEST-005",
            MovementType = StockMovementType.Transfer,
            ProductId = Guid.NewGuid(),
            SourceLocationId = _testLocations[0].Id.Value,
            DestinationLocationId = _testLocations[1].Id.Value
        };

        var command = new CreateStockMovementCommand(request);

        // Setup mocks
        _stockLocationRepositoryMock
            .Setup(x => x.GetByIdAsync(request.SourceLocationId))
            .ReturnsAsync(_testLocations[0]);

        _stockLocationRepositoryMock
            .Setup(x => x.GetByIdAsync(request.DestinationLocationId))
            .ReturnsAsync(_testLocations[1]);

        var savedMovement = new StockMovement
        {
            Id = StockMovementId.Of(Guid.NewGuid()),
            Quantity = request.Quantity,
            Reference = request.Reference,
            MovementType = request.MovementType,
            ProductId = request.ProductId,
            SourceLocationId = _testLocations[0].Id,
            DestinationLocationId = _testLocations[1].Id,
            SourceLocation = _testLocations[0],
            DestinationLocation = _testLocations[1],
            Date = DateTime.UtcNow
        };

        _stockMovementRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<StockMovement>()))
            .ReturnsAsync(savedMovement);

        _stockMovementRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(savedMovement);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Quantity.Should().Be(0);
    }

    [Fact]
    public async Task Handle_NegativeQuantity_ShouldCreateMovement()
    {
        // Arrange
        var request = new CreateStockMovementRequest
        {
            Quantity = -5,
            Reference = "MVT-TEST-006",
            MovementType = StockMovementType.Adjustment,
            ProductId = Guid.NewGuid(),
            SourceLocationId = _testLocations[0].Id.Value,
            DestinationLocationId = _testLocations[1].Id.Value
        };

        var command = new CreateStockMovementCommand(request);

        // Setup mocks
        _stockLocationRepositoryMock
            .Setup(x => x.GetByIdAsync(request.SourceLocationId))
            .ReturnsAsync(_testLocations[0]);

        _stockLocationRepositoryMock
            .Setup(x => x.GetByIdAsync(request.DestinationLocationId))
            .ReturnsAsync(_testLocations[1]);

        var savedMovement = new StockMovement
        {
            Id = StockMovementId.Of(Guid.NewGuid()),
            Quantity = request.Quantity,
            Reference = request.Reference,
            MovementType = request.MovementType,
            ProductId = request.ProductId,
            SourceLocationId = _testLocations[0].Id,
            DestinationLocationId = _testLocations[1].Id,
            SourceLocation = _testLocations[0],
            DestinationLocation = _testLocations[1],
            Date = DateTime.UtcNow
        };

        _stockMovementRepositoryMock
            .Setup(x => x.AddAsync(It.IsAny<StockMovement>()))
            .ReturnsAsync(savedMovement);

        _stockMovementRepositoryMock
            .Setup(x => x.GetByIdAsync(It.IsAny<Guid>()))
            .ReturnsAsync(savedMovement);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Quantity.Should().Be(-5);
    }
}