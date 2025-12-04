using IDR.Library.BuildingBlocks.CQRS;

namespace MagasinService.Application.Features.StockSlips.Queries.GetStockSlipById;

public record GetStockSlipByIdQuery(Guid Id) : IQuery<StockSlipDto>;