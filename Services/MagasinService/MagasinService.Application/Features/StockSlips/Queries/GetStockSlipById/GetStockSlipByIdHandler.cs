using IDR.Library.BuildingBlocks.CQRS;
using IDR.Library.BuildingBlocks.Exceptions;
using Mapster;

namespace MagasinService.Application.Features.StockSlips.Queries.GetStockSlipById;

public class GetStockSlipByIdHandler : IQueryHandler<GetStockSlipByIdQuery, StockSlipDto>
{
    private readonly IStockSlipRepository _stockSlipRepository;

    public GetStockSlipByIdHandler(IStockSlipRepository stockSlipRepository)
    {
        _stockSlipRepository = stockSlipRepository;
    }

    public async Task<StockSlipDto> Handle(GetStockSlipByIdQuery query, CancellationToken cancellationToken)
    {
        var slipId = StockSlipId.Of(query.Id);
        var slip = await _stockSlipRepository.GetSlipWithItemsAsync(slipId, cancellationToken);

        if (slip == null)
        {
            throw new NotFoundException("StockSlip", query.Id);
        }

        return slip.Adapt<StockSlipDto>();
    }
}