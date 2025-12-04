using IDR.Library.BuildingBlocks.CQRS;
using Mapster;

namespace MagasinService.Application.Features.StockSlips.Queries.GetStockSlips;

public class GetStockSlipsByBoutiqueHandler : IQueryHandler<GetStockSlipsByBoutiqueQuery, IReadOnlyList<StockSlipDto>>
{
    private readonly IStockSlipRepository _stockSlipRepository;

    public GetStockSlipsByBoutiqueHandler(IStockSlipRepository stockSlipRepository)
    {
        _stockSlipRepository = stockSlipRepository;
    }

    public async Task<IReadOnlyList<StockSlipDto>> Handle(GetStockSlipsByBoutiqueQuery query, CancellationToken cancellationToken)
    {
        var slips = await _stockSlipRepository.GetSlipsByBoutiqueAsync(query.BoutiqueId, cancellationToken);

        return slips.Adapt<IReadOnlyList<StockSlipDto>>();
    }
}