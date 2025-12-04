using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MagasinService.Application.Features.StockSlips.Commands.CreateStockSlip;
using MagasinService.Application.Features.StockSlips.DTOs;
using MagasinService.Domain.Entities;
using MagasinService.Domain.Enums;
using MagasinService.Domain.ValueObjects;
using MagasinService.Infrastructure.Data;
using MagasinService.Infrastructure.Repositories;

namespace MagasinService.Tests.Integration;

public class StockMovementIntegrationTests : IDisposable
{
    private readonly MagasinServiceDbContext _context;
    private readonly CreateStockSlipHandler _slipHandler;
    private readonly StockSlipRepository _slipRepository;
    private readonly StockLocationRepository _locationRepository;
    private readonly StockMovementRepository _movementRepository;
    private readonly UnitOfWork _unitOfWork;
    private readonly Guid _boutiqueId = Guid.NewGuid();

    public StockMovementIntegrationTests()
    {
        var options = new DbContextOptionsBuilder<MagasinServiceDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new MagasinServiceDbContext(options);
        _slipRepository = new StockSlipRepository(_context);
        _locationRepository = new StockLocationRepository(_context);
        _movementRepository = new StockMovementRepository(_context);
        _unitOfWork = new UnitOfWork(_context);

        _slipHandler = new CreateStockSlipHandler(
            _slipRepository,
            _locationRepository,
            _movementRepository,
            _unitOfWork
        );

        SetupTestData();
    }

    private void SetupTestData()
    {
        var locations = new List<StockLocation>
        {
            new StockLocation
            {
                Id = StockLocationId.Of(Guid.NewGuid()),
                Name = "Magasin Paris",
                Address = "1 Rue de Rivoli, Paris",
                Type = StockLocationType.Store,
                BoutiqueId = _boutiqueId
            },
            new StockLocation
            {
                Id = StockLocationId.Of(Guid.NewGuid()),
                Name = "Magasin Lyon",
                Address = "10 Place Bellecour, Lyon",
                Type = StockLocationType.Store,
                BoutiqueId = _boutiqueId
            },
            new StockLocation
            {
                Id = StockLocationId.Of(Guid.NewGuid()),
                Name = "Entrepôt Central",
                Address = "Zone Industrielle, Roissy",
                Type = StockLocationType.Warehouse,
                BoutiqueId = _boutiqueId
            }
        };

        _context.StockLocations.AddRange(locations);
        _context.SaveChanges();
    }

    [Fact]
    public async Task CompleteInterStoreMovement_ShouldCreateSlipAndMovements()
    {
        // Arrange
        var locations = await _context.StockLocations
            .Where(l => l.BoutiqueId == _boutiqueId)
            .ToListAsync();

        var sourceLocation = locations.First(l => l.Name == "Entrepôt Central");
        var destinationLocation = locations.First(l => l.Name == "Magasin Paris");

        var productIds = new[] { Guid.NewGuid(), Guid.NewGuid(), Guid.NewGuid() };

        var request = new CreateStockSlipRequest
        {
            Reference = "BS-2025-001",
            Note = "Réapprovisionnement mensuel du magasin Paris",
            BoutiqueId = _boutiqueId,
            SlipType = StockSlipType.Transfer,
            SourceLocationId = sourceLocation.Id.Value,
            DestinationLocationId = destinationLocation.Id.Value,
            Items = productIds.Select((id, index) => new CreateStockSlipItemRequest
            {
                ProductId = id,
                Quantity = (index + 1) * 10, // 10, 20, 30
                Note = $"Produit {index + 1}"
            }).ToList()
        };

        var command = new CreateStockSlipCommand(request);

        // Act
        var slipResult = await _slipHandler.Handle(command, CancellationToken.None);

        // Assert - Verify slip creation
        slipResult.Should().NotBeNull();
        slipResult.Reference.Should().Be("BS-2025-001");
        slipResult.Items.Should().HaveCount(3);

        // Verify database state
        var savedSlip = await _context.StockSlips
            .Include(s => s.StockSlipItems)
            .Include(s => s.SourceLocation)
            .Include(s => s.DestinationLocation)
            .FirstOrDefaultAsync(s => s.Id == StockSlipId.Of(slipResult.Id));

        savedSlip.Should().NotBeNull();
        savedSlip!.StockSlipItems.Should().HaveCount(3);
        savedSlip.SourceLocation.Name.Should().Be("Entrepôt Central");
        savedSlip.DestinationLocation!.Name.Should().Be("Magasin Paris");

        // Verify movements were created
        var movements = await _context.StockMovements
            .Include(m => m.SourceLocation)
            .Include(m => m.DestinationLocation)
            .Where(m => m.Reference == request.Reference)
            .OrderBy(m => m.ProductId)
            .ToListAsync();

        movements.Should().HaveCount(3);
        movements.All(m => m.MovementType == StockMovementType.Transfer).Should().BeTrue();
        movements.All(m => m.Date.Date == DateTime.UtcNow.Date).Should().BeTrue();

        // Verify quantities match items
        for (int i = 0; i < movements.Count; i++)
        {
            movements[i].Quantity.Should().Be((i + 1) * 10);
        }

        // Verify locations
        movements.All(m => m.SourceLocation.Name == "Entrepôt Central").Should().BeTrue();
        movements.All(m => m.DestinationLocation.Name == "Magasin Paris").Should().BeTrue();
    }

    [Fact]
    public async Task MultipleTransfers_ShouldMaintainDataIntegrity()
    {
        // Arrange
        var locations = await _context.StockLocations
            .Where(l => l.BoutiqueId == _boutiqueId)
            .ToListAsync();

        var warehouse = locations.First(l => l.Type == StockLocationType.Warehouse);
        var stores = locations.Where(l => l.Type == StockLocationType.Store).ToList();

        var productId = Guid.NewGuid();
        var transfers = new List<CreateStockSlipRequest>();

        // Create multiple transfers for the same product
        foreach (var store in stores)
        {
            transfers.Add(new CreateStockSlipRequest
            {
                Reference = $"BS-MULTI-{stores.IndexOf(store):000}",
                Note = $"Transfert vers {store.Name}",
                BoutiqueId = _boutiqueId,
                SlipType = StockSlipType.Transfer,
                SourceLocationId = warehouse.Id.Value,
                DestinationLocationId = store.Id.Value,
                Items = new List<CreateStockSlipItemRequest>
                {
                    new() { ProductId = productId, Quantity = 50, Note = "Stock initial" }
                }
            });
        }

        // Act
        var results = new List<StockSlipDto>();
        foreach (var transfer in transfers)
        {
            var command = new CreateStockSlipCommand(transfer);
            var result = await _slipHandler.Handle(command, CancellationToken.None);
            results.Add(result);
        }

        // Assert
        results.Should().HaveCount(stores.Count);

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
    public async Task EntrySlip_ShouldNotCreateMovements()
    {
        // Arrange
        var warehouse = await _context.StockLocations
            .FirstAsync(l => l.Type == StockLocationType.Warehouse && l.BoutiqueId == _boutiqueId);

        var request = new CreateStockSlipRequest
        {
            Reference = "BS-ENTRY-TEST",
            Note = "Réception de marchandises",
            BoutiqueId = _boutiqueId,
            SlipType = StockSlipType.Entry,
            SourceLocationId = warehouse.Id.Value,
            DestinationLocationId = null,
            Items = new List<CreateStockSlipItemRequest>
            {
                new() { ProductId = Guid.NewGuid(), Quantity = 100, Note = "Nouvelle livraison" }
            }
        };

        var command = new CreateStockSlipCommand(request);

        // Act
        var result = await _slipHandler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.SlipType.Should().Be(StockSlipType.Entry);

        // Verify no movements were created for entry slip
        var movements = await _context.StockMovements
            .Where(m => m.Reference == request.Reference)
            .ToListAsync();

        movements.Should().BeEmpty();

        // But slip should exist
        var slip = await _context.StockSlips
            .Include(s => s.StockSlipItems)
            .FirstAsync(s => s.Reference == request.Reference);

        slip.StockSlipItems.Should().HaveCount(1);
        slip.StockSlipItems.First().Quantity.Should().Be(100);
    }

    [Fact]
    public async Task ExitSlip_ShouldNotCreateMovements()
    {
        // Arrange
        var store = await _context.StockLocations
            .FirstAsync(l => l.Type == StockLocationType.Store && l.BoutiqueId == _boutiqueId);

        var request = new CreateStockSlipRequest
        {
            Reference = "BS-EXIT-TEST",
            Note = "Sortie pour vente",
            BoutiqueId = _boutiqueId,
            SlipType = StockSlipType.Exit,
            SourceLocationId = store.Id.Value,
            DestinationLocationId = null,
            Items = new List<CreateStockSlipItemRequest>
            {
                new() { ProductId = Guid.NewGuid(), Quantity = 25, Note = "Vente client" }
            }
        };

        var command = new CreateStockSlipCommand(request);

        // Act
        var result = await _slipHandler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.SlipType.Should().Be(StockSlipType.Exit);

        // Verify no movements were created for exit slip
        var movements = await _context.StockMovements
            .Where(m => m.Reference == request.Reference)
            .ToListAsync();

        movements.Should().BeEmpty();

        // But slip should exist
        var slip = await _context.StockSlips
            .Include(s => s.StockSlipItems)
            .FirstAsync(s => s.Reference == request.Reference);

        slip.StockSlipItems.Should().HaveCount(1);
        slip.StockSlipItems.First().Quantity.Should().Be(25);
    }

    public void Dispose()
    {
        _context?.Dispose();
    }
}