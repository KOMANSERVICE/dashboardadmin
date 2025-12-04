using IDR.Library.BuildingBlocks.CQRS;
using IDR.Library.BuildingBlocks.Exceptions;
using MagasinService.Application.Commons.Data;
using MagasinService.Application.Commons.Interfaces;
using MagasinService.Application.Features.StockMovements.DTOs;
using MagasinService.Domain.ValueObjects;
using Mapster;
using Microsoft.EntityFrameworkCore;

namespace MagasinService.Application.Features.StockMovements.Queries.GetStockMovementsByLocation;

public class GetStockMovementsByLocationHandler : IQueryHandler<GetStockMovementsByLocationQuery, GetStockMovementsByLocationResult>
{
    private readonly IMagasinServiceDbContext _dbContext;
    private readonly IStockLocationRepository _stockLocationRepository;

    public GetStockMovementsByLocationHandler(
        IMagasinServiceDbContext dbContext,
        IStockLocationRepository stockLocationRepository)
    {
        _dbContext = dbContext;
        _stockLocationRepository = stockLocationRepository;
    }

    public async Task<GetStockMovementsByLocationResult> Handle(
        GetStockMovementsByLocationQuery query,
        CancellationToken cancellationToken)
    {
        // Vérifier que le magasin existe
        var location = await _stockLocationRepository.GetByIdAsync(query.LocationId);
        if (location == null)
        {
            throw new NotFoundException("StockLocation", query.LocationId);
        }

        var locationId = StockLocationId.Of(query.LocationId);
        var movementsQuery = _dbContext.StockMovements
            .Include(m => m.SourceLocation)
            .Include(m => m.DestinationLocation)
            .AsQueryable();

        // Filtrer par location (entrant ou sortant)
        if (query.IncludeIncoming && query.IncludeOutgoing)
        {
            movementsQuery = movementsQuery.Where(m =>
                m.SourceLocationId == locationId || m.DestinationLocationId == locationId);
        }
        else if (query.IncludeIncoming)
        {
            movementsQuery = movementsQuery.Where(m => m.DestinationLocationId == locationId);
        }
        else if (query.IncludeOutgoing)
        {
            movementsQuery = movementsQuery.Where(m => m.SourceLocationId == locationId);
        }

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

        return new GetStockMovementsByLocationResult(movementDtos);
    }
}