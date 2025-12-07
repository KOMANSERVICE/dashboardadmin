using IDR.Library.BuildingBlocks.Exceptions;
using MagasinService.Application.Features.Magasins.DTOs;

namespace MagasinService.Application.Features.Magasins.Queries.GetOneMagasin;

public class GetOneMagasinHandler(
        IMagasinServiceDbContext _dbContext
    )
    : IQueryHandler<GetOneMagasinQuery, GetOneMagasinResult>
{
    public async Task<GetOneMagasinResult> Handle(GetOneMagasinQuery request, CancellationToken cancellationToken)
    {
        var stockLocation = await _dbContext.StockLocations
            .FirstOrDefaultAsync(
                s => s.BoutiqueId == request.BoutiqueId && s.Id == StockLocationId.Of(request.Id),
                cancellationToken);

        if (stockLocation is null)
        {
            throw new NotFoundException(nameof(StockLocation), request.Id);
        }

        var result = new StockLocationDTO
        {
            Id = stockLocation.Id.Value,
            Name = stockLocation.Name,
            Address = stockLocation.Address,
            Type = stockLocation.Type
        };

        return new GetOneMagasinResult(result);
    }
}
