using IDR.Library.BuildingBlocks.CQRS;
using MagasinService.Application.Features.StockMovements.Queries.GetStockMovementsByProduct;

namespace MagasinService.Api.Endpoints.StockMovements;

public record GetStockMovementsByProductRequest(
    DateTime? StartDate = null,
    DateTime? EndDate = null);

public class GetStockMovementsByProduct : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/stock-movements/product/{productId:guid}/boutique/{boutiqueId:guid}", async (
            Guid productId,
            Guid boutiqueId,
            [AsParameters] GetStockMovementsByProductRequest request,
            ISender sender) =>
        {
            var query = new GetStockMovementsByProductQuery(
                productId,
                boutiqueId,
                request.StartDate,
                request.EndDate);

            var result = await sender.Send(query);
            return Results.Ok(result);
        })
        .WithName("GetStockMovementsByProduct")
        .Produces<GetStockMovementsByProductResult>(StatusCodes.Status200OK)
        .WithSummary("Obtenir les mouvements de stock par produit")
        .WithDescription("Récupère tous les mouvements de stock d'un produit spécifique dans une boutique");
    }
}