using IDR.Library.BuildingBlocks.CQRS;
using MagasinService.Application.Features.Magasins.DTOs;

namespace MagasinService.Application.Features.Magasins.Commands.CreateMagasin;

public record CreateMagasinCommand(StockLocationDTO StockLocation,Guid BoutiqueId)
    : ICommand<CreateMagasinResult>;

public record CreateMagasinResult(Guid Id);
