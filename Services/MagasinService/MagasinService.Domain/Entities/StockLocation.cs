namespace MagasinService.Domain.Entities;

public class StockLocation : Entity<StockLocationId>
{
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public StockLocationType Type { get; set; }
    public Guid BoutiqueId { get; set; }

    public ICollection<StockMovement> SourceMovements { get; set; } = new List<StockMovement>();
    public ICollection<StockMovement> DestinationMovements { get; set; } = new List<StockMovement>();
    public ICollection<StockSlip> SourceSlips { get; set; } = new List<StockSlip>();
    public ICollection<StockSlip> DestinationSlips { get; set; } = new List<StockSlip>();
}
