namespace MagasinService.Domain.Entities;

public class StockSlip : Entity<StockSlipId>
{
    public string Reference { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string Note { get; set; } = string.Empty;
    public Guid BoutiqueId { get; set; }
    public StockSlipType SlipType { get; set; }
    public StockLocationId SourceLocationId { get; set; }
    public StockLocationId? DestinationLocationId { get; set; }

    public StockLocation SourceLocation { get; set; }
    public StockLocation? DestinationLocation { get; set; }

    public ICollection<StockSlipItem> StockSlipItems { get; set; } = new List<StockSlipItem>();
}