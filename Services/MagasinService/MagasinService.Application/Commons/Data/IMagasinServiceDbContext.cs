namespace MagasinService.Application.Commons.Data;

public interface IMagasinServiceDbContext
{
    public DbSet<StockLocation> StockLocations { get; }
}
