using Carter;
using IDR.Library.BuildingBlocks.CQRS;
using IDR.Library.BuildingBlocks.Responses;
using IDR.Library.Shared.Responses;
using MagasinService.Application.Features.StockMovements.Commands.CreateStockMovement;
using MagasinService.Domain.Enums;
using Mapster;

namespace MagasinService.Api.Endpoints.StockMovements;

public record CreateStockMovementRequest(
    Guid ProductId,
    Guid BoutiqueId,
    int Quantity,
    Guid SourceLocationId,
    Guid DestinationLocationId,
    string Reference,
    StockMovementType MovementType,
    string Note);

public record CreateStockMovementApiResponse(Guid Id, string Reference);

public class CreateStockMovement : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/stock-movements", async (CreateStockMovementRequest request, ISender sender) =>
        {
            var command = request.Adapt<CreateStockMovementCommand>();
            var result = await sender.Send(command) as CreateStockMovementResponse;

            if (result == null || !result.Success)
            {
                var errorMessage = result?.Message ?? "Erreur lors de la création du mouvement";
                var errorResponse = ResponseFactory.Success(new CreateStockMovementApiResponse(Guid.Empty, string.Empty), errorMessage, StatusCodes.Status400BadRequest);
                return Results.BadRequest(errorResponse);
            }

            var response = new CreateStockMovementApiResponse(result.Id, result.Reference);
            var baseResponse = ResponseFactory.Success(response, "Mouvement de stock créé avec succès", StatusCodes.Status201Created);

            return Results.Created($"/stock-movements/{result.Id}", baseResponse);
        })
        .WithName("CreateStockMovement")
        .WithTags("Stock Movements")
        .Produces<BaseResponse<CreateStockMovementApiResponse>>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .WithSummary("Créer un mouvement de stock")
        .WithDescription("Permet de créer un mouvement de stock entre deux magasins");
        //.RequireAuthorization();
    }
}