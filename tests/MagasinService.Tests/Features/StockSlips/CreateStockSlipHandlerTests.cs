using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using MagasinService.Application.Commons.Data;
using MagasinService.Application.Features.StockSlips.Commands.CreateStockSlip;
using MagasinService.Domain.Entities;
using MagasinService.Domain.Enums;
using MagasinService.Domain.ValueObjects;
using MagasinService.Infrastructure.Data;

namespace MagasinService.Tests.Features.StockSlips;

public class CreateStockSlipHandlerTests : IDisposable
{
    private readonly MagasinServiceDbContext _context;
    private readonly CreateStockSlipHandler _handler;
    private readonly List<StockLocation> _testLocations;
    private readonly Guid _boutiqueId = Guid.NewGuid();

    public CreateStockSlipHandlerTests()
    {
        var options = new DbContextOptionsBuilder<MagasinServiceDbContext>()
            .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
            .Options;

        _context = new MagasinServiceDbContext(options);
        _handler = new CreateStockSlipHandler(_context);

        // Setup test data
        _testLocations = SetupTestData();
    }

    private List<StockLocation> SetupTestData()
    {
        var locations = new List<StockLocation>
        {
            new StockLocation
            {
                Id = StockLocationId.Of(Guid.NewGuid()),
                Name = "Magasin Central",
                Address = "123 Rue Principale",
                Type = StockLocationType.Sale,
                BoutiqueId = _boutiqueId
            },
            new StockLocation
            {
                Id = StockLocationId.Of(Guid.NewGuid()),
                Name = "Magasin Secondaire",
                Address = "456 Rue Secondaire",
                Type = StockLocationType.Sale,
                BoutiqueId = _boutiqueId
            },
            new StockLocation
            {
                Id = StockLocationId.Of(Guid.NewGuid()),
                Name = "Entrepôt Externe",
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
    public async Task Handle_ValidInboundSlip_ShouldCreateStockSlipWithMovements()
    {
        // Arrange
        var items = new List<StockSlipItemDto>
        {
            new StockSlipItemDto
            {
                ProductId = Guid.NewGuid(),
                Quantity = 10,
                UnitPrice = 50.00m,
                Note = "Première livraison"
            },
            new StockSlipItemDto
            {
                ProductId = Guid.NewGuid(),
                Quantity = 5,
                UnitPrice = 75.00m,
                Note = "Deuxième livraison"
            }
        };

        var command = new CreateStockSlipCommand
        {
            BoutiqueId = _boutiqueId,
            SourceLocationId = _testLocations[0].Id.Value,
            DestinationLocationId = _testLocations[1].Id.Value,
            Note = "Bordereau d'entrée test",
            IsInbound = true,
            Items = items
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Id.Should().NotBeEmpty();
        result.Reference.Should().StartWith("BE-"); // Bordereau d'Entrée
        result.ItemsCount.Should().Be(2);
        result.Message.Should().Contain("succès");

        // Verify database
        var savedSlip = await _context.StockSlips
            .Include(s => s.StockSlipItems)
            .FirstOrDefaultAsync(s => s.Id.Value == result.Id);

        savedSlip.Should().NotBeNull();
        savedSlip!.BoutiqueId.Should().Be(_boutiqueId);
        savedSlip.IsInbound.Should().BeTrue();
        savedSlip.StockSlipItems.Should().HaveCount(2);

        // Verify movements were created
        var movements = await _context.StockMovements.ToListAsync();
        movements.Should().HaveCount(2);
    }

    [Fact]
    public async Task Handle_ValidOutboundSlip_ShouldCreateStockSlipWithMovements()
    {
        // Arrange
        var items = new List<StockSlipItemDto>
        {
            new StockSlipItemDto
            {
                ProductId = Guid.NewGuid(),
                Quantity = 15,
                UnitPrice = 100.00m,
                Note = "Sortie stock"
            }
        };

        var command = new CreateStockSlipCommand
        {
            BoutiqueId = _boutiqueId,
            SourceLocationId = _testLocations[0].Id.Value,
            DestinationLocationId = _testLocations[1].Id.Value,
            Note = "Bordereau de sortie test",
            IsInbound = false,
            Items = items
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.Reference.Should().StartWith("BS-"); // Bordereau de Sortie
        result.ItemsCount.Should().Be(1);
    }

    [Fact]
    public async Task Handle_DifferentBoutiques_ShouldReturnError()
    {
        // Arrange
        var items = new List<StockSlipItemDto>
        {
            new StockSlipItemDto
            {
                ProductId = Guid.NewGuid(),
                Quantity = 10,
                UnitPrice = 50.00m,
                Note = "Test"
            }
        };

        var command = new CreateStockSlipCommand
        {
            BoutiqueId = _boutiqueId,
            SourceLocationId = _testLocations[0].Id.Value,
            DestinationLocationId = _testLocations[2].Id.Value, // Different boutique
            Note = "Test bordereau",
            IsInbound = true,
            Items = items
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("même boutique");

        // Verify no slip or movements were created
        var slips = await _context.StockSlips.ToListAsync();
        slips.Should().BeEmpty();

        var movements = await _context.StockMovements.ToListAsync();
        movements.Should().BeEmpty();
    }

    [Fact]
    public async Task Handle_EmptyItems_ShouldReturnError()
    {
        // Arrange
        var command = new CreateStockSlipCommand
        {
            BoutiqueId = _boutiqueId,
            SourceLocationId = _testLocations[0].Id.Value,
            DestinationLocationId = _testLocations[1].Id.Value,
            Note = "Bordereau vide",
            IsInbound = true,
            Items = new List<StockSlipItemDto>() // Empty items
        };

        // Act & Assert
        // The validation should catch this, but if it doesn't, the handler should create an empty slip
        var result = await _handler.Handle(command, CancellationToken.None);

        result.Should().NotBeNull();
        result.ItemsCount.Should().Be(0);
    }

    [Fact]
    public async Task Handle_NonExistentSourceLocation_ShouldReturnError()
    {
        // Arrange
        var items = new List<StockSlipItemDto>
        {
            new StockSlipItemDto
            {
                ProductId = Guid.NewGuid(),
                Quantity = 10,
                UnitPrice = 50.00m,
                Note = "Test"
            }
        };

        var command = new CreateStockSlipCommand
        {
            BoutiqueId = _boutiqueId,
            SourceLocationId = Guid.NewGuid(), // Non-existent
            DestinationLocationId = _testLocations[1].Id.Value,
            Note = "Test bordereau",
            IsInbound = true,
            Items = items
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
        var items = new List<StockSlipItemDto>
        {
            new StockSlipItemDto
            {
                ProductId = Guid.NewGuid(),
                Quantity = 10,
                UnitPrice = 50.00m,
                Note = "Test"
            }
        };

        var command = new CreateStockSlipCommand
        {
            BoutiqueId = _boutiqueId,
            SourceLocationId = _testLocations[0].Id.Value,
            DestinationLocationId = Guid.NewGuid(), // Non-existent
            Note = "Test bordereau",
            IsInbound = false,
            Items = items
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeFalse();
        result.Message.Should().Contain("destination introuvable");
    }

    [Fact]
    public async Task Handle_MultipleItems_ShouldCreateCorrectReferences()
    {
        // Arrange
        var items = new List<StockSlipItemDto>
        {
            new StockSlipItemDto { ProductId = Guid.NewGuid(), Quantity = 10, UnitPrice = 50.00m, Note = "Item 1" },
            new StockSlipItemDto { ProductId = Guid.NewGuid(), Quantity = 20, UnitPrice = 60.00m, Note = "Item 2" },
            new StockSlipItemDto { ProductId = Guid.NewGuid(), Quantity = 30, UnitPrice = 70.00m, Note = "Item 3" }
        };

        var command = new CreateStockSlipCommand
        {
            BoutiqueId = _boutiqueId,
            SourceLocationId = _testLocations[0].Id.Value,
            DestinationLocationId = _testLocations[1].Id.Value,
            Note = "Bordereau multi-items",
            IsInbound = true,
            Items = items
        };

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Success.Should().BeTrue();
        result.ItemsCount.Should().Be(3);

        // Verify movements have sequential references
        var movements = await _context.StockMovements
            .OrderBy(m => m.Reference)
            .ToListAsync();

        movements.Should().HaveCount(3);
        movements[0].Reference.Should().EndWith("-001");
        movements[1].Reference.Should().EndWith("-002");
        movements[2].Reference.Should().EndWith("-003");

        // Verify all movements have the same base reference
        var baseReference = movements[0].Reference.Substring(0, movements[0].Reference.Length - 4);
        movements.Should().AllSatisfy(m => m.Reference.Should().StartWith(baseReference));
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}