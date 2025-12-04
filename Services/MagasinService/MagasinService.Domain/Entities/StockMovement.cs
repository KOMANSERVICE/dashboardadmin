namespace MagasinService.Domain.Entities;

public class StockMovement : Entity<StockMovementId>
{
    public int Quantity { get; set; }
    public DateTime Date { get; set; }
    public string Reference { get; set; } = string.Empty;
    public StockMovementType MovementType { get; set; }

    public Guid ProductId { get; set; }
    public StockLocationId SourceLocationId { get; set; }
    public StockLocationId DestinationLocationId { get; set; }

    public StockLocation SourceLocation { get; set; }
    public StockLocation DestinationLocation { get; set; }
}