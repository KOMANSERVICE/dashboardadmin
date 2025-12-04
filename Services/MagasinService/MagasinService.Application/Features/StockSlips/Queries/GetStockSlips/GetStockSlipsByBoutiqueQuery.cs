using IDR.Library.BuildingBlocks.CQRS;

namespace MagasinService.Application.Features.StockSlips.Queries.GetStockSlips;

public record GetStockSlipsByBoutiqueQuery(Guid BoutiqueId) : IQuery<IReadOnlyList<StockSlipDto>>;