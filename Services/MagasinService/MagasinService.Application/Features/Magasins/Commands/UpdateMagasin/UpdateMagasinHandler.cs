using MagasinService.Application.Commons.Data;
using MagasinService.Application.Features.Magasins.Commands.CreateMagasin;
using MagasinService.Application.Features.Magasins.DTOs;
using MagasinService.Domain.ValueObjects;

namespace MagasinService.Application.Features.Magasins.Commands.UpdateMagasin;

public class UpdateMagasinHandle(
        IGenericRepository<StockLocation> _stockLocationRepository,
        IMagasinServiceDbContext _dbContext,
        IUnitOfWork _unitOfWork
    )
    : ICommandHandler<UpdateMagasinCommand, UpdateMagasinResult>
{
    public async Task<UpdateMagasinResult> Handle(UpdateMagasinCommand request, CancellationToken cancellationToken)
    {

        var boutiqueId = request.BoutiqueId;
        var stockLocationId = request.StockLocationId;
        var stockLocationDTO = request.StockLocation;

        var stockLocation = await _dbContext.StockLocations.FirstOrDefaultAsync(s => s.Id == StockLocationId.Of(stockLocationId) && s.BoutiqueId == boutiqueId);

        stockLocation.Name = stockLocationDTO.Name;
        stockLocation.Address = stockLocationDTO.Address;


        _stockLocationRepository.UpdateData(stockLocation);
        await _unitOfWork.SaveChangesDataAsync(cancellationToken);

        return new UpdateMagasinResult(stockLocation.Id.Value);

    }

}
