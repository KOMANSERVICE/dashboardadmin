using IDR.Library.BuildingBlocks.CQRS;
using MagasinService.Application.Commons.Data;
using MagasinService.Application.Features.StockMovements.DTOs;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace MagasinService.Application.Features.StockMovements.Queries.GetStockMovementsByBoutique;

public class GetStockMovementsByBoutiqueHandler : IQueryHandler<GetStockMovementsByBoutiqueQuery, GetStockMovementsByBoutiqueResult>
{
    private readonly IMagasinServiceDbContext _dbContext;

    public GetStockMovementsByBoutiqueHandler(IMagasinServiceDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<GetStockMovementsByBoutiqueResult> Handle(
        GetStockMovementsByBoutiqueQuery query,
        CancellationToken cancellationToken)
    {
        var movementsQuery = _dbContext.StockMovements
            .Include(m => m.SourceLocation)
            .Include(m => m.DestinationLocation)
            .Where(m => m.SourceLocation.BoutiqueId == query.BoutiqueId
                || m.DestinationLocation.BoutiqueId == query.BoutiqueId);

        // Filtrer par dates
        if (query.StartDate.HasValue)
        {
            movementsQuery = movementsQuery.Where(m => m.Date >= query.StartDate.Value);
        }

        if (query.EndDate.HasValue)
        {
            movementsQuery = movementsQuery.Where(m => m.Date <= query.EndDate.Value);
        }

        // Compter le total avant pagination
        var totalCount = await movementsQuery.CountAsync(cancellationToken);

        // Appliquer la pagination et ordonner
        var movements = await movementsQuery
            .OrderByDescending(m => m.Date)
            .Skip((query.PageNumber - 1) * query.PageSize)
            .Take(query.PageSize)
            .ToListAsync(cancellationToken);

        var movementDtos = movements.Adapt<List<StockMovementDto>>();

        return new GetStockMovementsByBoutiqueResult(
            movementDtos,
            totalCount,
            query.PageNumber,
            query.PageSize);
    }
}