using IDR.Library.BuildingBlocks.CQRS;
using MagasinService.Application.Features.StockMovements.Queries.GetStockMovementsByBoutique;

namespace MagasinService.Api.Endpoints.StockMovements;

public record GetStockMovementsByBoutiqueRequest(
    DateTime? StartDate = null,
    DateTime? EndDate = null,
    int PageNumber = 1,
    int PageSize = 20);

public class GetStockMovementsByBoutique : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/stock-movements/boutique/{boutiqueId:guid}", async (
            Guid boutiqueId,
            [AsParameters] GetStockMovementsByBoutiqueRequest request,
            ISender sender) =>
        {
            var query = new GetStockMovementsByBoutiqueQuery(
                boutiqueId,
                request.StartDate,
                request.EndDate,
                request.PageNumber,
                request.PageSize);

            var result = await sender.Send(query);
            return Results.Ok(result);
        })
        .WithName("GetStockMovementsByBoutique")
        .Produces<GetStockMovementsByBoutiqueResult>(StatusCodes.Status200OK)
        .WithSummary("Obtenir les mouvements de stock par boutique")
        .WithDescription("Récupère tous les mouvements de stock d'une boutique avec pagination");
    }
}