using MagasinService.Application.Commons.Data;
using MagasinService.Application.Features.Magasins.DTOs;
using MagasinService.Domain.ValueObjects;

namespace MagasinService.Application.Features.Magasins.Commands.CreateMagasin;

public class CreateMagasinHandler(
        IGenericRepository<StockLocation> _stockLocationRepository,
        IUnitOfWork _unitOfWork
    )
    : ICommandHandler<CreateMagasinCommand, CreateMagasinResult>
{
    public async Task<CreateMagasinResult> Handle(CreateMagasinCommand request, CancellationToken cancellationToken)
    {

        //var stockLocation = request.StockLocation.Adapt<StockLocation>();
        //stockLocation.BoutiqueId = request.BoutiqueId;

        var stockLocationDto = request.StockLocation;
        var boutiqueId = request.BoutiqueId;

        var stockLocation = CreateStockLocation(stockLocationDto, boutiqueId);


        await _stockLocationRepository.AddDataAsync(stockLocation, cancellationToken);
        await _unitOfWork.SaveChangesDataAsync(cancellationToken);

        return new CreateMagasinResult(stockLocation.Id.Value);
    }

    private StockLocation CreateStockLocation(StockLocationDTO stockLocation, Guid BoutiqueId)
    {
        var id = StockLocationId.Of(Guid.NewGuid());

        return new StockLocation
        {
            Id = id,
            Name = stockLocation.Name,
            Address = stockLocation.Address,
            BoutiqueId = BoutiqueId,
            Type = stockLocation.Type
        };
    }
}
