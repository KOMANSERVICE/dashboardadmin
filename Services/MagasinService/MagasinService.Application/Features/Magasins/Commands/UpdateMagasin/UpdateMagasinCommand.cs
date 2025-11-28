using MagasinService.Application.Features.Magasins.DTOs;

namespace MagasinService.Application.Features.Magasins.Commands.UpdateMagasin;

public record UpdateMagasinCommand(StockLocationUpdateDTO StockLocation, Guid BoutiqueId, Guid StockLocationId)
    : ICommand<UpdateMagasinResult>;

public record UpdateMagasinResult(Guid Id);
