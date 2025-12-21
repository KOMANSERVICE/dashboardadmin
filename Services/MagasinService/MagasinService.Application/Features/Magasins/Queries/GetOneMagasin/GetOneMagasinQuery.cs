using MagasinService.Application.Features.Magasins.DTOs;

namespace MagasinService.Application.Features.Magasins.Queries.GetOneMagasin;

public record GetOneMagasinQuery(Guid BoutiqueId, Guid Id)
    : IQuery<GetOneMagasinResult>;

public record GetOneMagasinResult(StockLocationDTO StockLocation);
