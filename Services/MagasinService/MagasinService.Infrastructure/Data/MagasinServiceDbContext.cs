namespace MagasinService.Infrastructure.Data;

public class MagasinServiceDbContext : DbContext, IMagasinServiceDbContext
{
    public MagasinServiceDbContext(DbContextOptions<MagasinServiceDbContext> options)
        : base(options)
    {
    }
    public DbSet<StockLocation> StockLocations => Set<StockLocation>();
}
