namespace MagasinService.Application.Commons.Interfaces;

public interface IStockLocationRepository
{
    Task<StockLocation?> GetByIdAsync(Guid id);
    Task<StockLocation> AddAsync(StockLocation entity);
    Task<IReadOnlyList<StockLocation>> GetLocationsByBoutiqueAsync(Guid boutiqueId, CancellationToken cancellationToken = default);
    Task<StockLocation?> GetLocationWithMovementsAsync(StockLocationId locationId, CancellationToken cancellationToken = default);
}