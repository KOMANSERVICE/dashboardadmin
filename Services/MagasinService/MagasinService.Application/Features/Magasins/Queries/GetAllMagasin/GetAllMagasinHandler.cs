using MagasinService.Application.Features.Magasins.DTOs;
using System;
using System.Collections.Generic;
using System.Text;

namespace MagasinService.Application.Features.Magasins.Queries.GetAllMagasin;

public class GetAllMagasinHandler(
        IGenericRepository<StockLocation> _stockLocationRepository
    )
    : IQueryHandler<GetAllMagasinQuery, GetAllMagasinResult>
{
    public async Task<GetAllMagasinResult> Handle(GetAllMagasinQuery request, CancellationToken cancellationToken)
    {
        var boutiqueId = request.BoutiqueId;
        var stockLocations = await _stockLocationRepository.GetByConditionAsync(s => s.BoutiqueId == boutiqueId);

        var result = stockLocations.
            Select(s => new StockLocationDTO
            {
                Id = s.Id.Value,
                Name = s.Name,
                Address = s.Address,
                Type = s.Type
            })
            .ToList();

        return new GetAllMagasinResult(result);
    }
}
