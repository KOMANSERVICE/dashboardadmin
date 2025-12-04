using IDR.Library.BuildingBlocks.CQRS;
using MagasinService.Application.Features.StockMovements.Commands.CreateStockMovement;
using MagasinService.Application.Features.StockMovements.DTOs;

namespace MagasinService.Api.Endpoints.StockMovements;

public class CreateStockMovement : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/stock-movements", async (CreateStockMovementRequest request, ISender sender) =>
        {
            var command = new CreateStockMovementCommand(request);
            var result = await sender.Send(command);

            return Results.Created($"/stock-movements/{result.Id}", result);
        })
        .WithName("CreateStockMovement")
        .Produces<StockMovementDto>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .WithSummary("Créer un mouvement de stock inter-magasin")
        .WithDescription("Crée un nouveau mouvement de stock entre deux magasins de la même boutique");
    }
}