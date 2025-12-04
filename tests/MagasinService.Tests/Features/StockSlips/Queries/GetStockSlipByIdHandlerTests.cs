using FluentAssertions;
using MagasinService.Application.Commons.Interfaces;
using MagasinService.Application.Features.StockSlips.Queries.GetStockSlipById;
using MagasinService.Domain.Entities;
using MagasinService.Domain.Enums;
using MagasinService.Domain.ValueObjects;
using NSubstitute;

namespace MagasinService.Tests.Features.StockSlips.Queries;

public class GetStockSlipByIdHandlerTests
{
    private readonly IStockSlipRepository _stockSlipRepository;
    private readonly GetStockSlipByIdHandler _handler;

    public GetStockSlipByIdHandlerTests()
    {
        _stockSlipRepository = Substitute.For<IStockSlipRepository>();
        _handler = new GetStockSlipByIdHandler(_stockSlipRepository);
    }

    [Fact]
    public async Task Handle_ExistingSlipWithItems_ShouldReturnSlipDto()
    {
        // Arrange
        var slipId = Guid.NewGuid();
        var boutiqueId = Guid.NewGuid();
        var sourceLocationId = StockLocationId.Of(Guid.NewGuid());
        var destinationLocationId = StockLocationId.Of(Guid.NewGuid());

        var stockSlip = new StockSlip
        {
            Id = StockSlipId.Of(slipId),
            Reference = "BS-2025-001",
            Date = DateTime.UtcNow,
            Note = "Test slip",
            BoutiqueId = boutiqueId,
            SlipType = StockSlipType.Transfer,
            SourceLocationId = sourceLocationId,
            DestinationLocationId = destinationLocationId,
            SourceLocation = new StockLocation
            {
                Id = sourceLocationId,
                Name = "Source Warehouse",
                Address = "123 Source St",
                Type = StockLocationType.Store,
                BoutiqueId = boutiqueId
            },
            DestinationLocation = new StockLocation
            {
                Id = destinationLocationId,
                Name = "Destination Store",
                Address = "456 Dest Ave",
                Type = StockLocationType.Sale,
                BoutiqueId = boutiqueId
            },
            StockSlipItems = new List<StockSlipItem>
            {
                new StockSlipItem
                {
                    Id = StockSlipItemId.Of(Guid.NewGuid()),
                    StockSlipId = StockSlipId.Of(slipId),
                    ProductId = Guid.NewGuid(),
                    Quantity = 10,
                    Note = "Product 1"
                },
                new StockSlipItem
                {
                    Id = StockSlipItemId.Of(Guid.NewGuid()),
                    StockSlipId = StockSlipId.Of(slipId),
                    ProductId = Guid.NewGuid(),
                    Quantity = 5,
                    Note = "Product 2"
                }
            }
        };

        _stockSlipRepository.GetSlipWithItemsAsync(StockSlipId.Of(slipId), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<StockSlip?>(stockSlip));

        var query = new GetStockSlipByIdQuery(slipId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.StockSlip.Should().NotBeNull();
        result.StockSlip!.Id.Should().Be(slipId);
        result.StockSlip.Reference.Should().Be("BS-2025-001");
        result.StockSlip.SlipType.Should().Be(StockSlipType.Transfer);
        result.StockSlip.Items.Should().HaveCount(2);
        result.StockSlip.SourceLocation.Should().NotBeNull();
        result.StockSlip.DestinationLocation.Should().NotBeNull();
    }

    [Fact]
    public async Task Handle_NonExistingSlip_ShouldReturnNullSlip()
    {
        // Arrange
        var slipId = Guid.NewGuid();

        _stockSlipRepository.GetSlipWithItemsAsync(StockSlipId.Of(slipId), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<StockSlip?>(null));

        var query = new GetStockSlipByIdQuery(slipId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.StockSlip.Should().BeNull();
    }

    [Fact]
    public async Task Handle_SlipWithoutItems_ShouldReturnSlipWithEmptyItemsList()
    {
        // Arrange
        var slipId = Guid.NewGuid();
        var boutiqueId = Guid.NewGuid();
        var sourceLocationId = StockLocationId.Of(Guid.NewGuid());

        var stockSlip = new StockSlip
        {
            Id = StockSlipId.Of(slipId),
            Reference = "BS-2025-002",
            Date = DateTime.UtcNow,
            Note = "Empty slip",
            BoutiqueId = boutiqueId,
            SlipType = StockSlipType.Exit,
            SourceLocationId = sourceLocationId,
            SourceLocation = new StockLocation
            {
                Id = sourceLocationId,
                Name = "Warehouse",
                Address = "123 St",
                Type = StockLocationType.Store,
                BoutiqueId = boutiqueId
            },
            StockSlipItems = new List<StockSlipItem>() // Empty list
        };

        _stockSlipRepository.GetSlipWithItemsAsync(StockSlipId.Of(slipId), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult<StockSlip?>(stockSlip));

        var query = new GetStockSlipByIdQuery(slipId);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.StockSlip.Should().NotBeNull();
        result.StockSlip!.Items.Should().BeEmpty();
    }
}