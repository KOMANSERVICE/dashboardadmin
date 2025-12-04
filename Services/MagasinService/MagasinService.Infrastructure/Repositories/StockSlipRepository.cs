namespace MagasinService.Infrastructure.Repositories;

public class StockSlipRepository : IStockSlipRepository
{
    private readonly MagasinServiceDbContext _dbContext;

    public StockSlipRepository(MagasinServiceDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<StockSlip?> GetByIdAsync(Guid id)
    {
        return await _dbContext.StockSlips
            .Include(s => s.SourceLocation)
            .Include(s => s.DestinationLocation)
            .Include(s => s.StockSlipItems)
            .FirstOrDefaultAsync(s => s.Id == StockSlipId.Of(id));
    }

    public async Task<StockSlip> AddAsync(StockSlip entity)
    {
        await _dbContext.StockSlips.AddAsync(entity);
        return entity;
    }

    public async Task<StockSlip?> GetByReferenceAsync(string reference, CancellationToken cancellationToken = default)
    {
        return await _dbContext.StockSlips
            .Include(s => s.SourceLocation)
            .Include(s => s.DestinationLocation)
            .Include(s => s.StockSlipItems)
            .FirstOrDefaultAsync(s => s.Reference == reference, cancellationToken);
    }

    public async Task<IReadOnlyList<StockSlip>> GetSlipsByBoutiqueAsync(Guid boutiqueId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.StockSlips
            .Include(s => s.SourceLocation)
            .Include(s => s.DestinationLocation)
            .Where(s => s.BoutiqueId == boutiqueId)
            .OrderByDescending(s => s.Date)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<StockSlip>> GetSlipsByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default)
    {
        return await _dbContext.StockSlips
            .Include(s => s.SourceLocation)
            .Include(s => s.DestinationLocation)
            .Where(s => s.Date >= startDate && s.Date <= endDate)
            .OrderByDescending(s => s.Date)
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<StockSlip>> GetSlipsByLocationAsync(StockLocationId locationId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.StockSlips
            .Include(s => s.SourceLocation)
            .Include(s => s.DestinationLocation)
            .Where(s => s.SourceLocationId == locationId || s.DestinationLocationId == locationId)
            .OrderByDescending(s => s.Date)
            .ToListAsync(cancellationToken);
    }

    public async Task<StockSlip?> GetSlipWithItemsAsync(StockSlipId slipId, CancellationToken cancellationToken = default)
    {
        return await _dbContext.StockSlips
            .Include(s => s.SourceLocation)
            .Include(s => s.DestinationLocation)
            .Include(s => s.StockSlipItems)
            .FirstOrDefaultAsync(s => s.Id == slipId, cancellationToken);
    }
}