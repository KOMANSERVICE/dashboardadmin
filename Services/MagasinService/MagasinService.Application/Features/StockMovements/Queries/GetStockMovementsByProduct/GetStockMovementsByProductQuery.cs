using IDR.Library.BuildingBlocks.CQRS;
using MagasinService.Application.Features.StockMovements.DTOs;

namespace MagasinService.Application.Features.StockMovements.Queries.GetStockMovementsByProduct;

public record GetStockMovementsByProductQuery(
    Guid ProductId,
    Guid BoutiqueId,
    DateTime? StartDate = null,
    DateTime? EndDate = null
) : IQuery<GetStockMovementsByProductResult>;

public record GetStockMovementsByProductResult(List<StockMovementDto> Movements);