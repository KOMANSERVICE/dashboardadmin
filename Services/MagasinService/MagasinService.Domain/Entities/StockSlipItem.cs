namespace MagasinService.Domain.Entities;

public class StockSlipItem : Entity<StockSlipItemId>
{
    public StockSlipId StockSlipId { get; set; }
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public string Note { get; set; } = string.Empty;

    public StockSlip StockSlip { get; set; }
}