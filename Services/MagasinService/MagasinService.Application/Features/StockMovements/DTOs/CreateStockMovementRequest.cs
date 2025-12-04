namespace MagasinService.Application.Features.StockMovements.DTOs;

public record CreateStockMovementRequest
{
    public int Quantity { get; init; }
    public string Reference { get; init; } = string.Empty;
    public StockMovementType MovementType { get; init; }
    public Guid ProductId { get; init; }
    public Guid SourceLocationId { get; init; }
    public Guid DestinationLocationId { get; init; }
}