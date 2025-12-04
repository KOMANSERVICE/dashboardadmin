namespace MagasinService.Infrastructure.Repositories;

public class StockLocationRepository : IStockLocationRepository
{
    private readonly MagasinServiceDbContext _dbContext;

    public StockLocationRepository(MagasinServiceDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<StockLocation?> GetByIdAsync(Guid id)
    {
        return await _dbContext.StockLocations
            .FirstOrDefaultAsync(l => l.Id == StockLocationId.Of(id));
    }

    public async Task<StockLocation> AddAsync(StockLocation entity)
    {
        await _dbContext.StockLocations.AddAsync(entity);
        return entity;
    }

    public async Task<IReadOnlyList<StockLocation>> GetLocationsByBoutiqueAsync(Guid boutiqueId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.StockLocations
            .Where(l => l.BoutiqueId == boutiqueId)
            .OrderBy(l => l.Name)
            .ToListAsync(cancellationToken);
    }

    public async Task<StockLocation?> GetLocationWithMovementsAsync(StockLocationId locationId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.StockLocations
            .Include(l => l.SourceMovements)
                .ThenInclude(m => m.DestinationLocation)
            .Include(l => l.DestinationMovements)
                .ThenInclude(m => m.SourceLocation)
            .FirstOrDefaultAsync(l => l.Id == locationId, cancellationToken);
    }
}