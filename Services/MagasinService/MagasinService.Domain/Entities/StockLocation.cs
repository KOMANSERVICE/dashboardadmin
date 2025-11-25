namespace MagasinService.Domain.Entities;

public class StockLocation : Entity<StockLocationId>
{
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public StockLocationType Type { get; set; }
    public Guid BoutiqueId { get; set; }
}
