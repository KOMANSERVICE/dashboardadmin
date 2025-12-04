namespace MagasinService.Domain.Entities;

public class StockSlip : Entity<StockSlipId>
{
    public string Reference { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public string Note { get; set; } = string.Empty;
    public Guid BoutiqueId { get; set; } // Boutique provenant d'une autre application
    public bool IsInbound { get; set; } // true = bordereau d'entrée, false = bordereau de sortie

    // Navigation property
    public ICollection<StockSlipItem> StockSlipItems { get; set; } = new List<StockSlipItem>();
}