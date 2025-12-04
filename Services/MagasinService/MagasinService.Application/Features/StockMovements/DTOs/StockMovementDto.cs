namespace MagasinService.Application.Features.StockMovements.DTOs;

public record StockMovementDto
{
    public Guid Id { get; init; }
    public int Quantity { get; init; }
    public DateTime Date { get; init; }
    public string Reference { get; init; } = string.Empty;
    public StockMovementType MovementType { get; init; }
    public Guid ProductId { get; init; }
    public Guid SourceLocationId { get; init; }
    public Guid DestinationLocationId { get; init; }
    public StockLocationDTO? SourceLocation { get; init; }
    public StockLocationDTO? DestinationLocation { get; init; }
}