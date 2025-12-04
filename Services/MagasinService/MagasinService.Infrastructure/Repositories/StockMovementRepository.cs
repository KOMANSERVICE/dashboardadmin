namespace MagasinService.Infrastructure.Repositories;

public class StockMovementRepository : IStockMovementRepository
{
    private readonly MagasinServiceDbContext _dbContext;

    public StockMovementRepository(MagasinServiceDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<StockMovement?> GetByIdAsync(Guid id)
    {
        return await _dbContext.StockMovements
            .Include(m => m.SourceLocation)
            .Include(m => m.DestinationLocation)
            .FirstOrDefaultAsync(m => m.Id == StockMovementId.Of(id));
    }

    public async Task<StockMovement> AddAsync(StockMovement entity)
    {
        await _dbContext.StockMovements.AddAsync(entity);
        return entity;
    }

    public async Task<IReadOnlyList<StockMovement>> GetMovementsByProductAsync(Guid productId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.StockMovements
            .Include(m => m.SourceLocation)
            .Include(m => m.DestinationLocation)
            .Where(m => m.ProductId == productId)
            .OrderByDescending(m => m.Date)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<StockMovement>> GetMovementsByLocationAsync(StockLocationId locationId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.StockMovements
            .Include(m => m.SourceLocation)
            .Include(m => m.DestinationLocation)
            .Where(m => m.SourceLocationId == locationId || m.DestinationLocationId == locationId)
            .OrderByDescending(m => m.Date)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<StockMovement>> GetMovementsByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        return await _dbContext.StockMovements
            .Include(m => m.SourceLocation)
            .Include(m => m.DestinationLocation)
            .Where(m => m.Date >= startDate && m.Date <= endDate)
            .OrderByDescending(m => m.Date)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<StockMovement>> GetMovementsByBoutiqueAsync(Guid boutiqueId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.StockMovements
            .Include(m => m.SourceLocation)
            .Include(m => m.DestinationLocation)
            .Where(m => m.SourceLocation.BoutiqueId == boutiqueId || m.DestinationLocation.BoutiqueId == boutiqueId)
            .OrderByDescending(m => m.Date)
            .ToListAsync(cancellationToken);
    }
}