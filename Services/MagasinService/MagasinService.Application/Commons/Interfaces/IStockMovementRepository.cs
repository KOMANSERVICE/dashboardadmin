namespace MagasinService.Application.Commons.Interfaces;

public interface IStockMovementRepository
{
    Task<StockMovement?> GetByIdAsync(Guid id);
    Task<StockMovement> AddAsync(StockMovement entity);
    Task<IReadOnlyList<StockMovement>> GetMovementsByProductAsync(Guid productId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<StockMovement>> GetMovementsByLocationAsync(StockLocationId locationId, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<StockMovement>> GetMovementsByDateRangeAsync(DateTime startDate, DateTime endDate, CancellationToken cancellationToken = default);
    Task<IReadOnlyList<StockMovement>> GetMovementsByBoutiqueAsync(Guid boutiqueId, CancellationToken cancellationToken = default);
}