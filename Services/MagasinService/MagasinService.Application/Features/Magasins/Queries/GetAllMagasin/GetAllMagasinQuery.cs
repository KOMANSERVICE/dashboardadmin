using MagasinService.Application.Features.Magasins.DTOs;

namespace MagasinService.Application.Features.Magasins.Queries.GetAllMagasin;

public record GetAllMagasinQuery(Guid BoutiqueId)
    : IQuery<GetAllMagasinResult>;

public record GetAllMagasinResult(List<StockLocationDTO> StockLocations);

