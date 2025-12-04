using IDR.Library.BuildingBlocks.CQRS;
using MagasinService.Application.Features.Magasins.DTOs;

namespace MagasinService.Application.Features.StockMovements.Queries.GetStockMovements;

public record GetStockMovementsQuery : IQuery<GetStockMovementsResponse>
{
    public Guid? BoutiqueId { get; init; }
    public Guid? ProductId { get; init; }
    public Guid? LocationId { get; init; } // Pour chercher dans source ou destination
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }
    public StockMovementType? MovementType { get; init; }
}

public record GetStockMovementsResponse
{
    public List<StockMovementDto> Movements { get; init; } = new();
    public int TotalCount { get; init; }
}

public record StockMovementDto
{
    public Guid Id { get; init; }
    public Guid ProductId { get; init; }
    public int Quantity { get; init; }
    public DateTime Date { get; init; }
    public string Reference { get; init; } = string.Empty;
    public StockMovementType MovementType { get; init; }
    public StockLocationDTO SourceLocation { get; init; } = null!;
    public StockLocationDTO DestinationLocation { get; init; } = null!;
}