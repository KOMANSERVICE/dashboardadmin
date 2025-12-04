using IDR.Library.BuildingBlocks.CQRS;
using MagasinService.Application.Features.StockMovements.DTOs;

namespace MagasinService.Application.Features.StockMovements.Commands.CreateStockMovement;

public record CreateStockMovementCommand(CreateStockMovementRequest Request) : ICommand<StockMovementDto>;