using IDR.Library.BuildingBlocks.CQRS;
using MagasinService.Application.Features.StockMovements.DTOs;

namespace MagasinService.Application.Features.StockMovements.Queries.GetStockMovementsByBoutique;

public record GetStockMovementsByBoutiqueQuery(
    Guid BoutiqueId,
    DateTime? StartDate = null,
    DateTime? EndDate = null,
    int PageNumber = 1,
    int PageSize = 20
) : IQuery<GetStockMovementsByBoutiqueResult>;

public record GetStockMovementsByBoutiqueResult(
    List<StockMovementDto> Movements,
    int TotalCount,
    int PageNumber,
    int PageSize);