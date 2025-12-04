using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MagasinService.Application.Commons.Data;
using MagasinService.Application.Features.StockMovements.Commands.CreateStockMovement;
using MagasinService.Domain.Entities;
using MagasinService.Domain.Enums;
using MagasinService.Domain.ValueObjects;
using MagasinService.Infrastructure.Data;

namespace MagasinService.Tests.Features.StockMovements;

public class CreateStockMovementHandlerTests : IDisposable
{
    private readonly MagasinServiceDbContext _context;
    private readonly CreateStockMovementHandler _handler;
    private readonly List<StockLocation> _testLocations;

    public CreateStockMovementHandlerTests()
    {
        var options = new DbContextOptionsBuilder<MagasinServiceDbContext>()
            .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
            .Options;

        _context = new MagasinServiceDbContext(options);
        _handler = new CreateStockMovementHandler(_context);

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
                Type = StockLocationType.Sale,
                BoutiqueId = boutiqueId
            },
            new StockLocation
            {
                Id = StockLocationId.Of(Guid.NewGuid()),
                Name = "Magasin Secondaire",
                Address = "456 Rue Secondaire",
                Type = StockLocationType.Sale,
                BoutiqueId = boutiqueId
            },
            new StockLocation
            {
                Id = StockLocationId.Of(Guid.NewGuid()),
                Name = "Entrepôt",
                Address = "789 Zone Industrielle",
                Type = StockLocationType.Store,
                BoutiqueId = Guid.NewGuid() // Different boutique
            }
        };

        _context.StockLocations.AddRange(locations);
        _context.SaveChanges();

        return locations;
    }

    [Fact]
    public async Task Handle_ValidCommand_ShouldCreateStockMovement()
    {
        // Arrange
        var boutiqueId = _testLocations[0].BoutiqueId; // Same boutique for both locations
        var command = new CreateStockMovementCommand
        {
            Quantity = 10,
            Reference = "MVT-TEST-001",
            MovementType = StockMovementType.Transfer,
            ProductId = Guid.NewGuid(),
            BoutiqueId = boutiqueId,
            SourceLocationId = _testLocations[0].Id.Value,
            DestinationLocationId = _testLocations[1].Id.Value
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Id.Should().NotBeEmpty();
        result.Reference.Should().NotBeNullOrEmpty();
        result.Message.Should().Contain("succès");

        // Verify database
        var savedMovement = await _context.StockMovements
            .FirstOrDefaultAsync(x => x.Id.Value == result.Id);

        savedMovement.Should().NotBeNull();
        savedMovement!.Quantity.Should().Be(command.Quantity);
        savedMovement.ProductId.Should().Be(command.ProductId);
        savedMovement.SourceLocationId.Should().Be(_testLocations[0].Id);
        savedMovement.DestinationLocationId.Should().Be(_testLocations[1].Id);
    }

    [Fact]
    public async Task Handle_DifferentBoutiques_ShouldReturnError()
    {
        // Arrange
        var command = new CreateStockMovementCommand
        {
            Quantity = 10,
            Reference = "MVT-TEST-002",
            MovementType = StockMovementType.Transfer,
            ProductId = Guid.NewGuid(),
            BoutiqueId = _testLocations[0].BoutiqueId,
            SourceLocationId = _testLocations[0].Id.Value, // Boutique 1
            DestinationLocationId = _testLocations[2].Id.Value // Different boutique
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("même boutique");

        // Verify no movement was created
        var movements = await _context.StockMovements.ToListAsync();
        movements.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_NonExistentSourceLocation_ShouldReturnError()
    {
        // Arrange
        var command = new CreateStockMovementCommand
        {
            Quantity = 10,
            Reference = "MVT-TEST-003",
            MovementType = StockMovementType.Transfer,
            ProductId = Guid.NewGuid(),
            BoutiqueId = _testLocations[0].BoutiqueId,
            SourceLocationId = Guid.NewGuid(), // Non-existent
            DestinationLocationId = _testLocations[1].Id.Value
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("source introuvable");
    }

    [Fact]
    public async Task Handle_NonExistentDestinationLocation_ShouldReturnError()
    {
        // Arrange
        var command = new CreateStockMovementCommand
        {
            Quantity = 10,
            Reference = "MVT-TEST-004",
            MovementType = StockMovementType.Transfer,
            ProductId = Guid.NewGuid(),
            BoutiqueId = _testLocations[0].BoutiqueId,
            SourceLocationId = _testLocations[0].Id.Value,
            DestinationLocationId = Guid.NewGuid() // Non-existent
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("destination introuvable");
    }

    [Fact]
    public async Task Handle_EmptyReference_ShouldGenerateReference()
    {
        // Arrange
        var boutiqueId = _testLocations[0].BoutiqueId;
        var command = new CreateStockMovementCommand
        {
            Quantity = 10,
            Reference = "", // Empty reference
            MovementType = StockMovementType.Transfer,
            ProductId = Guid.NewGuid(),
            BoutiqueId = boutiqueId,
            SourceLocationId = _testLocations[0].Id.Value,
            DestinationLocationId = _testLocations[1].Id.Value
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Reference.Should().StartWith("MOV-");
        result.Reference.Should().MatchRegex(@"^MOV-\d{8}-[A-F0-9]{8}$");
    }

    [Fact]
    public async Task Handle_NegativeQuantity_ShouldCreateMovement()
    {
        // Arrange
        var boutiqueId = _testLocations[0].BoutiqueId;
        var command = new CreateStockMovementCommand
        {
            Quantity = -5,
            Reference = "MVT-TEST-005",
            MovementType = StockMovementType.Transfer,
            ProductId = Guid.NewGuid(),
            BoutiqueId = boutiqueId,
            SourceLocationId = _testLocations[0].Id.Value,
            DestinationLocationId = _testLocations[1].Id.Value
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();

        var savedMovement = await _context.StockMovements
            .FirstOrDefaultAsync(x => x.Id.Value == result.Id);
        savedMovement.Should().NotBeNull();
        savedMovement!.Quantity.Should().Be(-5);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}