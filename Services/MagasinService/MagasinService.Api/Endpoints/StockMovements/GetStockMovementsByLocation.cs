using IDR.Library.BuildingBlocks.CQRS;
using MagasinService.Application.Features.StockMovements.Queries.GetStockMovementsByLocation;

namespace MagasinService.Api.Endpoints.StockMovements;

public record GetStockMovementsByLocationRequest(
    bool IncludeIncoming = true,
    bool IncludeOutgoing = true,
    DateTime? StartDate = null,
    DateTime? EndDate = null);

public class GetStockMovementsByLocation : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/stock-movements/location/{locationId:guid}", async (
            Guid locationId,
            [AsParameters] GetStockMovementsByLocationRequest request,
            ISender sender) =>
        {
            var query = new GetStockMovementsByLocationQuery(
                locationId,
                request.IncludeIncoming,
                request.IncludeOutgoing,
                request.StartDate,
                request.EndDate);

            var result = await sender.Send(query);
            return Results.Ok(result);
        })
        .WithName("GetStockMovementsByLocation")
        .Produces<GetStockMovementsByLocationResult>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .WithSummary("Obtenir les mouvements de stock par magasin")
        .WithDescription("Récupère tous les mouvements de stock entrants et/ou sortants d'un magasin spécifique");
    }
}