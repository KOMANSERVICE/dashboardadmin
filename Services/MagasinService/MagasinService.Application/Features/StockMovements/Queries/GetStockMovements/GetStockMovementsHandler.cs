using IDR.Library.BuildingBlocks.CQRS;
using MagasinService.Application.Features.Magasins.DTOs;

namespace MagasinService.Application.Features.StockMovements.Queries.GetStockMovements;

public class GetStockMovementsHandler : IQueryHandler<GetStockMovementsQuery, GetStockMovementsResponse>
{
    private readonly IMagasinServiceDbContext _context;

    public GetStockMovementsHandler(IMagasinServiceDbContext context)
    {
        _context = context;
    }

    public async Task<GetStockMovementsResponse> Handle(
        GetStockMovementsQuery request,
        CancellationToken cancellationToken)
    {
        var query = _context.StockMovements
            .Include(m => m.SourceLocation)
            .Include(m => m.DestinationLocation)
            .AsQueryable();

        // Appliquer les filtres
        if (request.BoutiqueId.HasValue)
        {
            query = query.Where(m =>
                m.SourceLocation.BoutiqueId == request.BoutiqueId.Value ||
                m.DestinationLocation.BoutiqueId == request.BoutiqueId.Value);
        }

        if (request.ProductId.HasValue)
        {
            query = query.Where(m => m.ProductId == request.ProductId.Value);
        }

        if (request.LocationId.HasValue)
        {
            var locationId = StockLocationId.Of(request.LocationId.Value);
            query = query.Where(m =>
                m.SourceLocationId == locationId ||
                m.DestinationLocationId == locationId);
        }

        if (request.StartDate.HasValue)
        {
            query = query.Where(m => m.Date >= request.StartDate.Value);
        }

        if (request.EndDate.HasValue)
        {
            query = query.Where(m => m.Date <= request.EndDate.Value);
        }

        if (request.MovementType.HasValue)
        {
            query = query.Where(m => m.MovementType == request.MovementType.Value);
        }

        // Ordonner par date décroissante
        query = query.OrderByDescending(m => m.Date);

        var totalCount = await query.CountAsync(cancellationToken);
        var movements = await query
            .Select(m => new StockMovementDto
            {
                Id = m.Id.Value,
                ProductId = m.ProductId,
                Quantity = m.Quantity,
                Date = m.Date,
                Reference = m.Reference,
                MovementType = m.MovementType,
                SourceLocation = new StockLocationDTO
                {
                    Id = m.SourceLocation.Id.Value,
                    Name = m.SourceLocation.Name,
                    Address = m.SourceLocation.Address,
                    Type = m.SourceLocation.Type
                },
                DestinationLocation = new StockLocationDTO
                {
                    Id = m.DestinationLocation.Id.Value,
                    Name = m.DestinationLocation.Name,
                    Address = m.DestinationLocation.Address,
                    Type = m.DestinationLocation.Type
                }
            })
            .ToListAsync(cancellationToken);

        return new GetStockMovementsResponse
        {
            Movements = movements,
            TotalCount = totalCount
        };
    }
}