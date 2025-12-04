using Carter;
using IDR.Library.BuildingBlocks.CQRS;
using IDR.Library.BuildingBlocks.Responses;
using IDR.Library.Shared.Responses;
using MagasinService.Application.Features.StockMovements.Queries.GetStockMovements;
using MagasinService.Domain.Enums;

namespace MagasinService.Api.Endpoints.StockMovements;

public class GetStockMovements : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/stock-movements", async (
            Guid? boutiqueId,
            Guid? productId,
            Guid? locationId,
            DateTime? startDate,
            DateTime? endDate,
            StockMovementType? movementType,
            ISender sender) =>
        {
            var query = new GetStockMovementsQuery
            {
                BoutiqueId = boutiqueId,
                ProductId = productId,
                LocationId = locationId,
                StartDate = startDate,
                EndDate = endDate,
                MovementType = movementType
            };

            var result = await sender.Send(query);
            var baseResponse = ResponseFactory.Success(result, "Mouvements de stock récupérés avec succès");

            return Results.Ok(baseResponse);
        })
        .WithName("GetStockMovements")
        .WithTags("Stock Movements")
        .Produces<BaseResponse<GetStockMovementsResponse>>(StatusCodes.Status200OK)
        .WithSummary("Récupérer les mouvements de stock")
        .WithDescription("Permet de récupérer les mouvements de stock avec des filtres optionnels");
        //.RequireAuthorization();
    }
}