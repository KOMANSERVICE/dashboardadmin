namespace MagasinService.Application.Commons.Data;

public interface IMagasinServiceDbContext
{
    public DbSet<StockLocation> StockLocations { get; }
    public DbSet<StockMovement> StockMovements { get; }
    public DbSet<StockSlip> StockSlips { get; }
    public DbSet<StockSlipItem> StockSlipItems { get; }
}
