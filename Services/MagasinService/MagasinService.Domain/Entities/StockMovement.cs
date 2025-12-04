namespace MagasinService.Domain.Entities;

public class StockMovement : Entity<StockMovementId>
{
    public int Quantity { get; set; }
    public DateTime Date { get; set; }
    public string Reference { get; set; } = string.Empty;
    public StockMovementType MovementType { get; set; }

    public Guid ProductId { get; set; } // Produit provenant d'une autre application
    public StockLocationId SourceLocationId { get; set; } = null!;
    public StockLocationId DestinationLocationId { get; set; } = null!;

    // Navigation properties
    public StockLocation SourceLocation { get; set; } = null!;
    public StockLocation DestinationLocation { get; set; } = null!;
}