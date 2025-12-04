using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MagasinService.Application.Features.StockSlips.Commands.CreateStockSlip;
using MagasinService.Application.Features.StockMovements.Commands.CreateStockMovement;
using MagasinService.Domain.Entities;
using MagasinService.Domain.Enums;
using MagasinService.Domain.ValueObjects;
using MagasinService.Infrastructure.Data;

namespace MagasinService.Tests.Integration;

public class StockMovementIntegrationTests : IDisposable
{
    private readonly MagasinServiceDbContext _context;
    private readonly CreateStockSlipHandler _slipHandler;
    private readonly CreateStockMovementHandler _movementHandler;
    private readonly Guid _boutiqueId = Guid.NewGuid();
    private readonly List<StockLocation> _testLocations;

    public StockMovementIntegrationTests()
    {
        var options = new DbContextOptionsBuilder<MagasinServiceDbContext>()
            .UseInMemoryDatabase(databaseName: $"IntegrationTestDb_{Guid.NewGuid()}")
            .Options;

        _context = new MagasinServiceDbContext(options);
        _slipHandler = new CreateStockSlipHandler(_context);
        _movementHandler = new CreateStockMovementHandler(_context);

        _testLocations = SetupTestData();
    }

    private List<StockLocation> SetupTestData()
    {
        var locations = new List<StockLocation>
        {
            new StockLocation
            {
                Id = StockLocationId.Of(Guid.NewGuid()),
                Name = "Magasin Paris",
                Address = "1 Rue de Rivoli, Paris",
                Type = StockLocationType.Sale,
                BoutiqueId = _boutiqueId
            },
            new StockLocation
            {
                Id = StockLocationId.Of(Guid.NewGuid()),
                Name = "Magasin Lyon",
                Address = "10 Place Bellecour, Lyon",
                Type = StockLocationType.Sale,
                BoutiqueId = _boutiqueId
            },
            new StockLocation
            {
                Id = StockLocationId.Of(Guid.NewGuid()),
                Name = "Entrepôt Central",
                Address = "Zone Industrielle, Roissy",
                Type = StockLocationType.Store,
                BoutiqueId = _boutiqueId
            },
            new StockLocation
            {
                Id = StockLocationId.Of(Guid.NewGuid()),
                Name = "Magasin Externe",
                Address = "Autre adresse",
                Type = StockLocationType.Sale,
                BoutiqueId = Guid.NewGuid() // Different boutique
            }
        };

        _context.StockLocations.AddRange(locations);
        _context.SaveChanges();

        return locations;
    }

    [Fact]
    public async Task CompleteInterStoreMovement_ShouldCreateSlipAndMovements()
    {
        // Arrange
        var sourceLocation = _testLocations.First(l => l.Name == "Entrepôt Central");
        var destinationLocation = _testLocations.First(l => l.Name == "Magasin Paris");

        var productIds = new[] { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };

        var slipCommand = new CreateStockSlipCommand
        {
            BoutiqueId = _boutiqueId,
            SourceLocationId = sourceLocation.Id.Value,
            DestinationLocationId = destinationLocation.Id.Value,
            Note = "Réapprovisionnement mensuel du magasin Paris",
            IsInbound = false, // Sortie de l'entrepôt
            Items = productIds.Select((id, index) => new StockSlipItemDto
            {
                ProductId = id,
                Quantity = (index + 1) * 10, // 10, 20, 30
                UnitPrice = 50.00m + (index * 10),
                Note = $"Produit {index + 1}"
            }).ToList()
        };

        // Act
        var slipResult = await _slipHandler.Handle(slipCommand, CancellationToken.None);

        // Assert - Verify slip creation
        slipResult.Should().NotBeNull();
        slipResult.Success.Should().BeTrue();
        slipResult.Reference.Should().StartWith("BS-"); // Bordereau de sortie
        slipResult.ItemsCount.Should().Be(3);

        // Verify database state
        var savedSlip = await _context.StockSlips
            .Include(s => s.StockSlipItems)
            .FirstOrDefaultAsync(s => s.Id.Value == slipResult.Id);

        savedSlip.Should().NotBeNull();
        savedSlip!.StockSlipItems.Should().HaveCount(3);
        savedSlip.BoutiqueId.Should().Be(_boutiqueId);
        savedSlip.IsInbound.Should().BeFalse();

        // Verify movements were created
        var movements = await _context.StockMovements
            .Include(m => m.SourceLocation)
            .Include(m => m.DestinationLocation)
            .OrderBy(m => m.Reference)
            .ToListAsync();

        movements.Should().HaveCount(3);
        movements.All(m => m.SourceLocationId == sourceLocation.Id).Should().BeTrue();
        movements.All(m => m.DestinationLocationId == destinationLocation.Id).Should().BeTrue();
        movements.All(m => m.Date.Date == DateTime.UtcNow.Date).Should().BeTrue();

        // Verify quantities match items
        var orderedMovements = movements.OrderBy(m => m.Quantity).ToList();
        orderedMovements[0].Quantity.Should().Be(10);
        orderedMovements[1].Quantity.Should().Be(20);
        orderedMovements[2].Quantity.Should().Be(30);

        // Verify slip items are linked to movements
        foreach (var slipItem in savedSlip.StockSlipItems)
        {
            var relatedMovement = movements.FirstOrDefault(m => m.Id == slipItem.StockMovementId);
            relatedMovement.Should().NotBeNull();
            relatedMovement!.ProductId.Should().Be(slipItem.ProductId);
            relatedMovement.Quantity.Should().Be(slipItem.Quantity);
        }
    }

    [Fact]
    public async Task MultipleTransfers_ShouldMaintainDataIntegrity()
    {
        // Arrange
        var warehouse = _testLocations.First(l => l.Type == StockLocationType.Store && l.Name.Contains("Entrepôt"));
        var stores = _testLocations.Where(l => l.Type == StockLocationType.Sale && l.BoutiqueId == _boutiqueId).ToList();

        var productId = Guid.NewGuid();
        var transfers = new List<CreateStockSlipCommand>();

        // Create multiple transfers for the same product
        foreach (var store in stores)
        {
            transfers.Add(new CreateStockSlipCommand
            {
                BoutiqueId = _boutiqueId,
                SourceLocationId = warehouse.Id.Value,
                DestinationLocationId = store.Id.Value,
                Note = $"Transfert vers {store.Name}",
                IsInbound = false,
                Items = new List<StockSlipItemDto>
                {
                    new() { ProductId = productId, Quantity = 50, UnitPrice = 100.00m, Note = "Stock initial" }
                }
            });
        }

        // Act
        var results = new List<CreateStockSlipResponse>();
        foreach (var transfer in transfers)
        {
            var result = await _slipHandler.Handle(transfer, CancellationToken.None);
            results.Add(result);
        }

        // Assert
        results.Should().HaveCount(stores.Count);
        results.All(r => r.Success).Should().BeTrue();

        // Verify all movements for the product
        var allMovements = await _context.StockMovements
            .Where(m => m.ProductId == productId)
            .ToListAsync();

        allMovements.Should().HaveCount(stores.Count);
        allMovements.Sum(m => m.Quantity).Should().Be(stores.Count * 50);
        allMovements.All(m => m.SourceLocationId == warehouse.Id).Should().BeTrue();
        allMovements.Select(m => m.DestinationLocationId).Distinct().Count().Should().Be(stores.Count);
    }

    [Fact]
    public async Task DirectMovementWithoutSlip_ShouldCreateMovement()
    {
        // Arrange
        var sourceLocation = _testLocations.First(l => l.Name == "Magasin Paris");
        var destinationLocation = _testLocations.First(l => l.Name == "Magasin Lyon");

        var movementCommand = new CreateStockMovementCommand
        {
            ProductId = Guid.NewGuid(),
            BoutiqueId = _boutiqueId,
            Quantity = 15,
            SourceLocationId = sourceLocation.Id.Value,
            DestinationLocationId = destinationLocation.Id.Value,
            Reference = "MOV-DIRECT-001",
            MovementType = StockMovementType.Transfer,
            Note = "Transfert direct entre magasins"
        };

        // Act
        var result = await _movementHandler.Handle(movementCommand, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Reference.Should().Be("MOV-DIRECT-001");

        // Verify movement in database
        var savedMovement = await _context.StockMovements
            .Include(m => m.SourceLocation)
            .Include(m => m.DestinationLocation)
            .FirstOrDefaultAsync(m => m.Id.Value == result.Id);

        savedMovement.Should().NotBeNull();
        savedMovement!.ProductId.Should().Be(movementCommand.ProductId);
        savedMovement.Quantity.Should().Be(movementCommand.Quantity);
        savedMovement.SourceLocation.Name.Should().Be("Magasin Paris");
        savedMovement.DestinationLocation.Name.Should().Be("Magasin Lyon");
        savedMovement.MovementType.Should().Be(StockMovementType.Transfer);
    }

    [Fact]
    public async Task CrossBoutiqueTransfer_ShouldFail()
    {
        // Arrange
        var internalLocation = _testLocations.First(l => l.BoutiqueId == _boutiqueId);
        var externalLocation = _testLocations.First(l => l.BoutiqueId != _boutiqueId);

        var movementCommand = new CreateStockMovementCommand
        {
            ProductId = Guid.NewGuid(),
            BoutiqueId = _boutiqueId,
            Quantity = 10,
            SourceLocationId = internalLocation.Id.Value,
            DestinationLocationId = externalLocation.Id.Value,
            Reference = "MOV-CROSS-001",
            MovementType = StockMovementType.Transfer,
            Note = "Tentative de transfert inter-boutique"
        };

        // Act
        var result = await _movementHandler.Handle(movementCommand, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("même boutique");

        // Verify no movement was created
        var movements = await _context.StockMovements
            .Where(m => m.Reference == "MOV-CROSS-001")
            .ToListAsync();

        movements.Should().BeEmpty();
    }

    [Fact]
    public async Task CompleteWorkflow_FromWarehouseToMultipleStores()
    {
        // Arrange
        var warehouse = _testLocations.First(l => l.Type == StockLocationType.Store && l.Name.Contains("Entrepôt"));
        var stores = _testLocations.Where(l => l.Type == StockLocationType.Sale && l.BoutiqueId == _boutiqueId).ToList();
        var productId = Guid.NewGuid();

        // Act & Assert - Create initial stock slip from warehouse to first store
        var initialSlipCommand = new CreateStockSlipCommand
        {
            BoutiqueId = _boutiqueId,
            SourceLocationId = warehouse.Id.Value,
            DestinationLocationId = stores[0].Id.Value,
            Note = "Distribution initiale",
            IsInbound = false,
            Items = new List<StockSlipItemDto>
            {
                new() { ProductId = productId, Quantity = 100, UnitPrice = 50.00m, Note = "Stock initial" }
            }
        };

        var initialResult = await _slipHandler.Handle(initialSlipCommand, CancellationToken.None);
        initialResult.Success.Should().BeTrue();

        // Then transfer between stores
        var interStoreCommand = new CreateStockMovementCommand
        {
            ProductId = productId,
            BoutiqueId = _boutiqueId,
            Quantity = 30,
            SourceLocationId = stores[0].Id.Value,
            DestinationLocationId = stores[1].Id.Value,
            Reference = "MOV-INTER-STORE",
            MovementType = StockMovementType.Transfer,
            Note = "Rééquilibrage des stocks"
        };

        var interStoreResult = await _movementHandler.Handle(interStoreCommand, CancellationToken.None);
        interStoreResult.Success.Should().BeTrue();

        // Verify complete movement history
        var allMovements = await _context.StockMovements
            .Where(m => m.ProductId == productId)
            .OrderBy(m => m.Date)
            .ToListAsync();

        allMovements.Should().HaveCount(2);

        // First movement: warehouse to store 1 (100 units)
        allMovements[0].SourceLocationId.Should().Be(warehouse.Id);
        allMovements[0].DestinationLocationId.Should().Be(stores[0].Id);
        allMovements[0].Quantity.Should().Be(100);

        // Second movement: store 1 to store 2 (30 units)
        allMovements[1].SourceLocationId.Should().Be(stores[0].Id);
        allMovements[1].DestinationLocationId.Should().Be(stores[1].Id);
        allMovements[1].Quantity.Should().Be(30);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}