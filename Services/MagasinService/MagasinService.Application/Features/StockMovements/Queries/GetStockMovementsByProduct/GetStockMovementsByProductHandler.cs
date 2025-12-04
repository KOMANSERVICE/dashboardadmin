using IDR.Library.BuildingBlocks.CQRS;
using MagasinService.Application.Commons.Data;
using MagasinService.Application.Features.StockMovements.DTOs;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace MagasinService.Application.Features.StockMovements.Queries.GetStockMovementsByProduct;

public class GetStockMovementsByProductHandler : IQueryHandler<GetStockMovementsByProductQuery, GetStockMovementsByProductResult>
{
    private readonly IMagasinServiceDbContext _dbContext;

    public GetStockMovementsByProductHandler(IMagasinServiceDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<GetStockMovementsByProductResult> Handle(
        GetStockMovementsByProductQuery query,
        CancellationToken cancellationToken)
    {
        var movementsQuery = _dbContext.StockMovements
            .Include(m => m.SourceLocation)
            .Include(m => m.DestinationLocation)
            .Where(m => m.ProductId == query.ProductId
                && (m.SourceLocation.BoutiqueId == query.BoutiqueId
                    || m.DestinationLocation.BoutiqueId == query.BoutiqueId));

        // Filtrer par dates
        if (query.StartDate.HasValue)
        {
            movementsQuery = movementsQuery.Where(m => m.Date >= query.StartDate.Value);
        }

        if (query.EndDate.HasValue)
        {
            movementsQuery = movementsQuery.Where(m => m.Date <= query.EndDate.Value);
        }

        // Ordonner par date décroissante
        var movements = await movementsQuery
            .OrderByDescending(m => m.Date)
            .ToListAsync(cancellationToken);

        var movementDtos = movements.Adapt<List<StockMovementDto>>();

        return new GetStockMovementsByProductResult(movementDtos);
    }
}