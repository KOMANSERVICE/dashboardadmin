using IDR.Library.BuildingBlocks.CQRS;

namespace MagasinService.Application.Features.StockSlips.Commands.CreateStockSlip;

public record CreateStockSlipCommand(CreateStockSlipRequest Request) : ICommand<StockSlipDto>;