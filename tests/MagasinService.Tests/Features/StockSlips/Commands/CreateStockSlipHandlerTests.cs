using FluentAssertions;
using MagasinService.Application.Commons.Interfaces;
using MagasinService.Application.Features.StockSlips.Commands.CreateStockSlip;
using MagasinService.Application.Features.StockSlips.DTOs;
using MagasinService.Domain.Entities;
using MagasinService.Domain.Enums;
using MagasinService.Domain.ValueObjects;
using NSubstitute;
using IDR.Library.BuildingBlocks.Repositories;
using IDR.Library.BuildingBlocks.Exceptions;

namespace MagasinService.Tests.Features.StockSlips.Commands;

public class CreateStockSlipHandlerTests
{
    private readonly IStockLocationRepository _stockLocationRepository;
    private readonly IStockSlipRepository _stockSlipRepository;
    private readonly IStockMovementRepository _stockMovementRepository;
    private readonly IUnitOfWork _unitOfWork;
    private readonly CreateStockSlipHandler _handler;

    public CreateStockSlipHandlerTests()
    {
        _stockLocationRepository = Substitute.For<IStockLocationRepository>();
        _stockSlipRepository = Substitute.For<IStockSlipRepository>();
        _stockMovementRepository = Substitute.For<IStockMovementRepository>();
        _unitOfWork = Substitute.For<IUnitOfWork>();

        _handler = new CreateStockSlipHandler(
            _stockSlipRepository,
            _stockLocationRepository,
            _stockMovementRepository,
            _unitOfWork
        );
    }

    [Fact]
    public async Task Handle_ValidTransferSlip_ShouldCreateSlipWithMovements()
    {
        // Arrange
        var boutiqueId = Guid.NewGuid();
        var sourceLocationId = Guid.NewGuid();
        var destinationLocationId = Guid.NewGuid();
        var productId1 = Guid.NewGuid();
        var productId2 = Guid.NewGuid();

        var request = new CreateStockSlipRequest
        {
            Reference = "BS-2025-001",
            Note = "Transfer test",
            BoutiqueId = boutiqueId,
            SlipType = StockSlipType.Transfer,
            SourceLocationId = sourceLocationId,
            DestinationLocationId = destinationLocationId,
            Items = new List<CreateStockSlipItemRequest>
            {
                new() { ProductId = productId1, Quantity = 10, Note = "Item 1" },
                new() { ProductId = productId2, Quantity = 5, Note = "Item 2" }
            }
        };

        var command = new CreateStockSlipCommand(request);

        var sourceLocation = new StockLocation
        {
            Id = StockLocationId.Of(sourceLocationId),
            Name = "Source Warehouse",
            Address = "123 Source St",
            Type = StockLocationType.Store,
            BoutiqueId = boutiqueId
        };

        var destinationLocation = new StockLocation
        {
            Id = StockLocationId.Of(destinationLocationId),
            Name = "Destination Store",
            Address = "456 Dest Ave",
            Type = StockLocationType.Sale,
            BoutiqueId = boutiqueId
        };

        _stockSlipRepository.GetByReferenceAsync(request.Reference, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<StockSlip?>(null));

        _stockLocationRepository.GetByIdAsync(sourceLocationId, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<StockLocation?>(sourceLocation));

        _stockLocationRepository.GetByIdAsync(destinationLocationId, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<StockLocation?>(destinationLocation));

        _stockSlipRepository.AddAsync(Arg.Any<StockSlip>(), Arg.Any<CancellationToken>())
            .Returns(args => Task.FromResult(args.Arg<StockSlip>()));

        _stockMovementRepository.AddAsync(Arg.Any<StockMovement>(), Arg.Any<CancellationToken>())
            .Returns(args => Task.FromResult(args.Arg<StockMovement>()));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Reference.Should().Be(request.Reference);
        result.SlipType.Should().Be(StockSlipType.Transfer);
        result.BoutiqueId.Should().Be(boutiqueId);
        result.Items.Should().HaveCount(2);

        await _stockSlipRepository.Received(1).AddAsync(Arg.Any<StockSlip>(), Arg.Any<CancellationToken>());
        await _stockMovementRepository.Received(2).AddAsync(Arg.Any<StockMovement>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesDataAsync(Arg.Any<CancellationToken>());
    }

    [Fact]
    public async Task Handle_DuplicateReference_ShouldThrowConflictException()
    {
        // Arrange
        var existingSlip = new StockSlip
        {
            Id = StockSlipId.Of(Guid.NewGuid()),
            Reference = "BS-2025-001",
            Date = DateTime.UtcNow,
            BoutiqueId = Guid.NewGuid()
        };

        var request = new CreateStockSlipRequest
        {
            Reference = "BS-2025-001",
            BoutiqueId = Guid.NewGuid(),
            SlipType = StockSlipType.Entry,
            SourceLocationId = Guid.NewGuid(),
            Items = new List<CreateStockSlipItemRequest>
            {
                new() { ProductId = Guid.NewGuid(), Quantity = 10 }
            }
        };

        var command = new CreateStockSlipCommand(request);

        _stockSlipRepository.GetByReferenceAsync(request.Reference, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<StockSlip?>(existingSlip));

        // Act & Assert
        var act = async () => await _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<ConflictException>()
            .WithMessage($"Un bordereau avec la référence '{request.Reference}' existe déjà");
    }

    [Fact]
    public async Task Handle_SourceLocationNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var request = new CreateStockSlipRequest
        {
            Reference = "BS-2025-002",
            BoutiqueId = Guid.NewGuid(),
            SlipType = StockSlipType.Exit,
            SourceLocationId = Guid.NewGuid(),
            Items = new List<CreateStockSlipItemRequest>
            {
                new() { ProductId = Guid.NewGuid(), Quantity = 10 }
            }
        };

        var command = new CreateStockSlipCommand(request);

        _stockSlipRepository.GetByReferenceAsync(request.Reference, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<StockSlip?>(null));

        _stockLocationRepository.GetByIdAsync(request.SourceLocationId, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<StockLocation?>(null));

        // Act & Assert
        var act = async () => await _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<NotFoundException>()
            .WithMessage($"Magasin source avec l'ID '{request.SourceLocationId}' non trouvé");
    }

    [Fact]
    public async Task Handle_TransferWithDifferentBoutiques_ShouldThrowDomainException()
    {
        // Arrange
        var sourceLocationId = Guid.NewGuid();
        var destinationLocationId = Guid.NewGuid();

        var request = new CreateStockSlipRequest
        {
            Reference = "BS-2025-003",
            BoutiqueId = Guid.NewGuid(),
            SlipType = StockSlipType.Transfer,
            SourceLocationId = sourceLocationId,
            DestinationLocationId = destinationLocationId,
            Items = new List<CreateStockSlipItemRequest>
            {
                new() { ProductId = Guid.NewGuid(), Quantity = 10 }
            }
        };

        var command = new CreateStockSlipCommand(request);

        var sourceLocation = new StockLocation
        {
            Id = StockLocationId.Of(sourceLocationId),
            Name = "Source",
            Address = "123 St",
            BoutiqueId = Guid.NewGuid() // Different boutique
        };

        var destinationLocation = new StockLocation
        {
            Id = StockLocationId.Of(destinationLocationId),
            Name = "Destination",
            Address = "456 Ave",
            BoutiqueId = Guid.NewGuid() // Another different boutique
        };

        _stockSlipRepository.GetByReferenceAsync(request.Reference, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<StockSlip?>(null));

        _stockLocationRepository.GetByIdAsync(sourceLocationId, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<StockLocation?>(sourceLocation));

        _stockLocationRepository.GetByIdAsync(destinationLocationId, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<StockLocation?>(destinationLocation));

        // Act & Assert
        var act = async () => await _handler.Handle(command, CancellationToken.None);
        await act.Should().ThrowAsync<DomainException>()
            .WithMessage("Les magasins source et destination doivent appartenir à la même boutique");
    }

    [Fact]
    public async Task Handle_EntrySlip_ShouldNotCreateMovements()
    {
        // Arrange
        var sourceLocationId = Guid.NewGuid();
        var boutiqueId = Guid.NewGuid();

        var request = new CreateStockSlipRequest
        {
            Reference = "BS-ENTRY-001",
            BoutiqueId = boutiqueId,
            SlipType = StockSlipType.Entry,
            SourceLocationId = sourceLocationId,
            Items = new List<CreateStockSlipItemRequest>
            {
                new() { ProductId = Guid.NewGuid(), Quantity = 20 }
            }
        };

        var command = new CreateStockSlipCommand(request);

        var sourceLocation = new StockLocation
        {
            Id = StockLocationId.Of(sourceLocationId),
            Name = "Warehouse",
            Address = "123 St",
            BoutiqueId = boutiqueId
        };

        _stockSlipRepository.GetByReferenceAsync(request.Reference, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<StockSlip?>(null));

        _stockLocationRepository.GetByIdAsync(sourceLocationId, Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<StockLocation?>(sourceLocation));

        _stockSlipRepository.AddAsync(Arg.Any<StockSlip>(), Arg.Any<CancellationToken>())
            .Returns(args => Task.FromResult(args.Arg<StockSlip>()));

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.SlipType.Should().Be(StockSlipType.Entry);

        await _stockSlipRepository.Received(1).AddAsync(Arg.Any<StockSlip>(), Arg.Any<CancellationToken>());
        await _stockMovementRepository.DidNotReceive().AddAsync(Arg.Any<StockMovement>(), Arg.Any<CancellationToken>());
        await _unitOfWork.Received(1).SaveChangesDataAsync(Arg.Any<CancellationToken>());
    }
}