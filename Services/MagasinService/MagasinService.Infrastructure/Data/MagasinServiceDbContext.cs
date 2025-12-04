using System.Reflection;

namespace MagasinService.Infrastructure.Data;

public class MagasinServiceDbContext : DbContext, IMagasinServiceDbContext
{
    public MagasinServiceDbContext(DbContextOptions<MagasinServiceDbContext> options)
        : base(options)
    {
    }
    public DbSet<StockLocation> StockLocations => Set<StockLocation>();
    public DbSet<StockMovement> StockMovements => Set<StockMovement>();
    public DbSet<StockSlip> StockSlips => Set<StockSlip>();
    public DbSet<StockSlipItem> StockSlipItems => Set<StockSlipItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(modelBuilder);
    }
}
