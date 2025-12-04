namespace MagasinService.Application.Commons.Interfaces;

public interface IStockSlipRepository
{
    Task<StockSlip?> GetByIdAsync(Guid id);
    Task<StockSlip> AddAsync(StockSlip entity);
    Task<StockSlip?> GetByReferenceAsync(string reference, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<StockSlip>> GetSlipsByBoutiqueAsync(Guid boutiqueId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<StockSlip>> GetSlipsByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<StockSlip>> GetSlipsByLocationAsync(StockLocationId locationId, CancellationToken cancellationToken = default);
    Task<StockSlip?> GetSlipWithItemsAsync(StockSlipId slipId, CancellationToken cancellationToken = default);
}