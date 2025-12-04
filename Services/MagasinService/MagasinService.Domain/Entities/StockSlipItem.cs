namespace MagasinService.Domain.Entities;

public class StockSlipItem : Entity<StockSlipItemId>
{
    public Guid ProductId { get; set; } // Produit provenant d'une autre application
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public string Note { get; set; } = string.Empty;

    // Foreign keys
    public StockSlipId StockSlipId { get; set; } = null!;
    public StockMovementId StockMovementId { get; set; } = null!;

    // Navigation properties
    public StockSlip StockSlip { get; set; } = null!;
    public StockMovement StockMovement { get; set; } = null!;
}