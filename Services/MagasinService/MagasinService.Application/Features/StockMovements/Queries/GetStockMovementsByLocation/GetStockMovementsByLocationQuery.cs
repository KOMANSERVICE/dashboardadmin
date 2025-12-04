using IDR.Library.BuildingBlocks.CQRS;
using MagasinService.Application.Features.StockMovements.DTOs;

namespace MagasinService.Application.Features.StockMovements.Queries.GetStockMovementsByLocation;

public record GetStockMovementsByLocationQuery(
    Guid LocationId,
    bool IncludeIncoming = true,
    bool IncludeOutgoing = true,
    DateTime? StartDate = null,
    DateTime? EndDate = null
) : IQuery<GetStockMovementsByLocationResult>;

public record GetStockMovementsByLocationResult(List<StockMovementDto> Movements);