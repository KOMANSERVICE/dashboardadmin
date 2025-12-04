using FluentAssertions;
using Moq;
using MagasinService.Application.Commons.Interfaces;
using MagasinService.Application.Features.StockSlips.Commands.CreateStockSlip;
using MagasinService.Application.Features.StockSlips.DTOs;
using MagasinService.Domain.Entities;
using MagasinService.Domain.Enums;
using MagasinService.Domain.Exceptions;
using MagasinService.Domain.ValueObjects;
using IDR.Library.BuildingBlocks.Exceptions;
using Mapster;

namespace MagasinService.Tests.Features.StockSlips;

public class CreateStockSlipHandlerTests
{
    private readonly Mock<IStockSlipRepository> _stockSlipRepositoryMock;
    private readonly Mock<IStockLocationRepository> _stockLocationRepositoryMock;
    private readonly Mock<IStockMovementRepository> _stockMovementRepositoryMock;
    private readonly Mock<IUnitOfWork> _unitOfWorkMock;
    private readonly CreateStockSlipHandler _handler;
    private readonly List<StockLocation> _testLocations;
    private readonly Guid _boutiqueId = Guid.NewGuid();

    public CreateStockSlipHandlerTests()
    {
        _stockSlipRepositoryMock = new Mock<IStockSlipRepository>();
        _stockLocationRepositoryMock = new Mock<IStockLocationRepository>();
        _stockMovementRepositoryMock = new Mock<IStockMovementRepository>();
        _unitOfWorkMock = new Mock<IUnitOfWork>();

        _handler = new CreateStockSlipHandler(
            _stockSlipRepositoryMock.Object,
            _stockLocationRepositoryMock.Object,
            _stockMovementRepositoryMock.Object,
            _unitOfWorkMock.Object
        );

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
                Address = "100 Avenue Centrale",
                Type = StockLocationType.Store,
                BoutiqueId = _boutiqueId
            },
            new StockLocation
            {
                Id = StockLocationId.Of(Guid.NewGuid()),
                Name = "Magasin Nord",
                Address = "200 Route du Nord",
                Type = StockLocationType.Store,
                BoutiqueId = _boutiqueId
            },
            new StockLocation
            {
                Id = StockLocationId.Of(Guid.NewGuid()),
                Name = "Entrepôt Principal",
                Address = "300 Zone Logistique",
                Type = StockLocationType.Warehouse,
                BoutiqueId = Guid.NewGuid() // Different boutique
            }
        };

        return locations;
    }

    [Fact]
    public async Task Handle_ValidTransferSlip_ShouldCreateSlipAndMovements()
    {
        // Arrange
        var productId1 = Guid.NewGuid();
        var productId2 = Guid.NewGuid();

        var request = new CreateStockSlipRequest
        {
            Reference = "BS-TEST-001",
            Note = "Transfert test",
            BoutiqueId = _boutiqueId,
            SlipType = StockSlipType.Transfer,
            SourceLocationId = _testLocations[0].Id.Value,
            DestinationLocationId = _testLocations[1].Id.Value,
            Items = new List<CreateStockSlipItemRequest>
            {
                new() { ProductId = productId1, Quantity = 10, Note = "Produit 1" },
                new() { ProductId = productId2, Quantity = 20, Note = "Produit 2" }
            }
        };

        var command = new CreateStockSlipCommand(request);

        // Setup mocks
        _stockSlipRepositoryMock
            .Setup(x => x.GetByReferenceAsync(request.Reference, It.IsAny<CancellationToken>()))
            .ReturnsAsync((StockSlip?)null);

        _stockLocationRepositoryMock
            .Setup(x => x.GetByIdAsync(request.SourceLocationId))
            .ReturnsAsync(_testLocations[0]);

        _stockLocationRepositoryMock
            .Setup(x => x.GetByIdAsync(request.DestinationLocationId!.Value))
            .ReturnsAsync(_testLocations[1]);

        var createdSlip = new StockSlip
        {
            Id = StockSlipId.Of(Guid.NewGuid()),
            Reference = request.Reference,
            Date = DateTime.UtcNow,
            Note = request.Note,
            BoutiqueId = request.BoutiqueId,
            SlipType = request.SlipType,
            SourceLocationId = _testLocations[0].Id,
            DestinationLocationId = _testLocations[1].Id,
            SourceLocation = _testLocations[0],
            DestinationLocation = _testLocations[1],
            StockSlipItems = new List<StockSlipItem>
            {
                new() {
                    Id = StockSlipItemId.Of(Guid.NewGuid()),
                    ProductId = productId1,
                    Quantity = 10,
                    Note = "Produit 1"
                },
                new() {
                    Id = StockSlipItemId.Of(Guid.NewGuid()),
                    ProductId = productId2,
                    Quantity = 20,
                    Note = "Produit 2"
                }
            }
        };

        _stockSlipRepositoryMock
            .Setup(x => x.GetSlipWithItemsAsync(It.IsAny<StockSlipId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdSlip);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().NotBeEmpty();
        result.Reference.Should().Be(request.Reference);
        result.Note.Should().Be(request.Note);
        result.BoutiqueId.Should().Be(request.BoutiqueId);
        result.SlipType.Should().Be(request.SlipType);
        result.Items.Should().HaveCount(2);

        // Verify mocks
        _stockSlipRepositoryMock.Verify(x => x.GetByReferenceAsync(request.Reference, It.IsAny<CancellationToken>()), Times.Once);
        _stockLocationRepositoryMock.Verify(x => x.GetByIdAsync(request.SourceLocationId), Times.Once);
        _stockLocationRepositoryMock.Verify(x => x.GetByIdAsync(request.DestinationLocationId!.Value), Times.Once);
        _stockSlipRepositoryMock.Verify(x => x.AddAsync(It.IsAny<StockSlip>()), Times.Once);
        _stockMovementRepositoryMock.Verify(x => x.AddAsync(It.IsAny<StockMovement>()), Times.Exactly(2)); // 2 items
        _unitOfWorkMock.Verify(x => x.SaveChangesDataAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_EntrySlip_ShouldCreateWithoutDestination()
    {
        // Arrange
        var request = new CreateStockSlipRequest
        {
            Reference = "BS-ENTRY-001",
            Note = "Réception marchandise",
            BoutiqueId = _boutiqueId,
            SlipType = StockSlipType.Entry,
            SourceLocationId = _testLocations[0].Id.Value,
            DestinationLocationId = null, // No destination for entry
            Items = new List<CreateStockSlipItemRequest>
            {
                new() { ProductId = Guid.NewGuid(), Quantity = 50, Note = "Nouvelle livraison" }
            }
        };

        var command = new CreateStockSlipCommand(request);

        // Setup mocks
        _stockSlipRepositoryMock
            .Setup(x => x.GetByReferenceAsync(request.Reference, It.IsAny<CancellationToken>()))
            .ReturnsAsync((StockSlip?)null);

        _stockLocationRepositoryMock
            .Setup(x => x.GetByIdAsync(request.SourceLocationId))
            .ReturnsAsync(_testLocations[0]);

        var createdSlip = new StockSlip
        {
            Id = StockSlipId.Of(Guid.NewGuid()),
            Reference = request.Reference,
            Date = DateTime.UtcNow,
            Note = request.Note,
            BoutiqueId = request.BoutiqueId,
            SlipType = request.SlipType,
            SourceLocationId = _testLocations[0].Id,
            DestinationLocationId = null,
            SourceLocation = _testLocations[0],
            DestinationLocation = null,
            StockSlipItems = new List<StockSlipItem>
            {
                new() {
                    Id = StockSlipItemId.Of(Guid.NewGuid()),
                    ProductId = request.Items[0].ProductId,
                    Quantity = 50,
                    Note = "Nouvelle livraison"
                }
            }
        };

        _stockSlipRepositoryMock
            .Setup(x => x.GetSlipWithItemsAsync(It.IsAny<StockSlipId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdSlip);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.SlipType.Should().Be(StockSlipType.Entry);
        result.DestinationLocationId.Should().BeNull();

        // Verify mocks
        _stockMovementRepositoryMock.Verify(x => x.AddAsync(It.IsAny<StockMovement>()), Times.Never); // No movements for Entry type without destination
    }

    [Fact]
    public async Task Handle_DuplicateReference_ShouldThrowDomainException()
    {
        // Arrange
        var existingSlip = new StockSlip
        {
            Id = StockSlipId.Of(Guid.NewGuid()),
            Reference = "BS-DUPLICATE-001",
            Date = DateTime.UtcNow,
            Note = "Existing slip",
            BoutiqueId = _boutiqueId,
            SlipType = StockSlipType.Transfer,
            SourceLocationId = _testLocations[0].Id,
            DestinationLocationId = _testLocations[1].Id
        };

        var request = new CreateStockSlipRequest
        {
            Reference = "BS-DUPLICATE-001", // Same reference
            Note = "Duplicate test",
            BoutiqueId = _boutiqueId,
            SlipType = StockSlipType.Transfer,
            SourceLocationId = _testLocations[0].Id.Value,
            DestinationLocationId = _testLocations[1].Id.Value,
            Items = new List<CreateStockSlipItemRequest>
            {
                new() { ProductId = Guid.NewGuid(), Quantity = 10 }
            }
        };

        var command = new CreateStockSlipCommand(request);

        // Setup mocks
        _stockSlipRepositoryMock
            .Setup(x => x.GetByReferenceAsync(request.Reference, It.IsAny<CancellationToken>()))
            .ReturnsAsync(existingSlip);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("*référence*existe déjà*");

        // Verify mocks
        _stockLocationRepositoryMock.Verify(x => x.GetByIdAsync(It.IsAny<Guid>()), Times.Never);
        _stockSlipRepositoryMock.Verify(x => x.AddAsync(It.IsAny<StockSlip>()), Times.Never);
        _unitOfWorkMock.Verify(x => x.SaveChangesDataAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_TransferWithNonexistentDestination_ShouldThrowNotFoundException()
    {
        // Arrange
        var request = new CreateStockSlipRequest
        {
            Reference = "BS-NOTFOUND-001",
            Note = "Transfer avec destination inexistante",
            BoutiqueId = _boutiqueId,
            SlipType = StockSlipType.Transfer,
            SourceLocationId = _testLocations[0].Id.Value,
            DestinationLocationId = Guid.NewGuid(), // Non-existent
            Items = new List<CreateStockSlipItemRequest>
            {
                new() { ProductId = Guid.NewGuid(), Quantity = 10 }
            }
        };

        var command = new CreateStockSlipCommand(request);

        // Setup mocks
        _stockSlipRepositoryMock
            .Setup(x => x.GetByReferenceAsync(request.Reference, It.IsAny<CancellationToken>()))
            .ReturnsAsync((StockSlip?)null);

        _stockLocationRepositoryMock
            .Setup(x => x.GetByIdAsync(request.SourceLocationId))
            .ReturnsAsync(_testLocations[0]);

        _stockLocationRepositoryMock
            .Setup(x => x.GetByIdAsync(request.DestinationLocationId!.Value))
            .ReturnsAsync((StockLocation?)null);

        // Act
        Func<Task> act = async () => await _handler.Handle(command, CancellationToken.None);

        // Assert
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage("*StockLocation*");

        // Verify mocks
        _stockSlipRepositoryMock.Verify(x => x.AddAsync(It.IsAny<StockSlip>()), Times.Never);
        _unitOfWorkMock.Verify(x => x.SaveChangesDataAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Handle_ExitSlip_ShouldCreateWithoutDestination()
    {
        // Arrange
        var request = new CreateStockSlipRequest
        {
            Reference = "BS-EXIT-001",
            Note = "Sortie de stock",
            BoutiqueId = _boutiqueId,
            SlipType = StockSlipType.Exit,
            SourceLocationId = _testLocations[0].Id.Value,
            DestinationLocationId = null,
            Items = new List<CreateStockSlipItemRequest>
            {
                new() { ProductId = Guid.NewGuid(), Quantity = 30, Note = "Sortie pour vente" }
            }
        };

        var command = new CreateStockSlipCommand(request);

        // Setup mocks
        _stockSlipRepositoryMock
            .Setup(x => x.GetByReferenceAsync(request.Reference, It.IsAny<CancellationToken>()))
            .ReturnsAsync((StockSlip?)null);

        _stockLocationRepositoryMock
            .Setup(x => x.GetByIdAsync(request.SourceLocationId))
            .ReturnsAsync(_testLocations[0]);

        var createdSlip = new StockSlip
        {
            Id = StockSlipId.Of(Guid.NewGuid()),
            Reference = request.Reference,
            Date = DateTime.UtcNow,
            Note = request.Note,
            BoutiqueId = request.BoutiqueId,
            SlipType = request.SlipType,
            SourceLocationId = _testLocations[0].Id,
            DestinationLocationId = null,
            SourceLocation = _testLocations[0],
            DestinationLocation = null,
            StockSlipItems = new List<StockSlipItem>
            {
                new() {
                    Id = StockSlipItemId.Of(Guid.NewGuid()),
                    ProductId = request.Items[0].ProductId,
                    Quantity = 30,
                    Note = "Sortie pour vente"
                }
            }
        };

        _stockSlipRepositoryMock
            .Setup(x => x.GetSlipWithItemsAsync(It.IsAny<StockSlipId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdSlip);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.SlipType.Should().Be(StockSlipType.Exit);
        result.DestinationLocationId.Should().BeNull();

        // Verify mocks
        _stockMovementRepositoryMock.Verify(x => x.AddAsync(It.IsAny<StockMovement>()), Times.Never); // No movements for Exit type
    }

    [Fact]
    public async Task Handle_MultipleItems_ShouldCreateMultipleMovements()
    {
        // Arrange
        var items = new List<CreateStockSlipItemRequest>();
        for (int i = 0; i < 5; i++)
        {
            items.Add(new CreateStockSlipItemRequest
            {
                ProductId = Guid.NewGuid(),
                Quantity = 10 + i,
                Note = $"Item {i + 1}"
            });
        }

        var request = new CreateStockSlipRequest
        {
            Reference = "BS-MULTI-001",
            Note = "Multiple items transfer",
            BoutiqueId = _boutiqueId,
            SlipType = StockSlipType.Transfer,
            SourceLocationId = _testLocations[0].Id.Value,
            DestinationLocationId = _testLocations[1].Id.Value,
            Items = items
        };

        var command = new CreateStockSlipCommand(request);

        // Setup mocks
        _stockSlipRepositoryMock
            .Setup(x => x.GetByReferenceAsync(request.Reference, It.IsAny<CancellationToken>()))
            .ReturnsAsync((StockSlip?)null);

        _stockLocationRepositoryMock
            .Setup(x => x.GetByIdAsync(request.SourceLocationId))
            .ReturnsAsync(_testLocations[0]);

        _stockLocationRepositoryMock
            .Setup(x => x.GetByIdAsync(request.DestinationLocationId!.Value))
            .ReturnsAsync(_testLocations[1]);

        var stockSlipItems = items.Select(i => new StockSlipItem
        {
            Id = StockSlipItemId.Of(Guid.NewGuid()),
            ProductId = i.ProductId,
            Quantity = i.Quantity,
            Note = i.Note
        }).ToList();

        var createdSlip = new StockSlip
        {
            Id = StockSlipId.Of(Guid.NewGuid()),
            Reference = request.Reference,
            Date = DateTime.UtcNow,
            Note = request.Note,
            BoutiqueId = request.BoutiqueId,
            SlipType = request.SlipType,
            SourceLocationId = _testLocations[0].Id,
            DestinationLocationId = _testLocations[1].Id,
            SourceLocation = _testLocations[0],
            DestinationLocation = _testLocations[1],
            StockSlipItems = stockSlipItems
        };

        _stockSlipRepositoryMock
            .Setup(x => x.GetSlipWithItemsAsync(It.IsAny<StockSlipId>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(createdSlip);

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Items.Should().HaveCount(5);

        // Verify mocks
        _stockMovementRepositoryMock.Verify(x => x.AddAsync(It.IsAny<StockMovement>()), Times.Exactly(5)); // 5 movements for 5 items
    }
}