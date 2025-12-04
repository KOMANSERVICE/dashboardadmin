using IDR.Library.BuildingBlocks.CQRS;

namespace MagasinService.Application.Features.StockMovements.Commands.CreateStockMovement;

public record CreateStockMovementCommand : ICommand<CreateStockMovementResponse>
{
    public Guid ProductId { get; init; } // Produit provenant d'une autre application
    public Guid BoutiqueId { get; init; } // Boutique provenant d'une autre application
    public int Quantity { get; init; }
    public Guid SourceLocationId { get; init; }
    public Guid DestinationLocationId { get; init; }
    public string Reference { get; init; } = string.Empty;
    public StockMovementType MovementType { get; init; }
    public string Note { get; init; } = string.Empty;
}

public record CreateStockMovementResponse
{
    public Guid Id { get; init; }
    public string Reference { get; init; } = string.Empty;
    public bool Success { get; init; }
    public string? Message { get; init; }
}