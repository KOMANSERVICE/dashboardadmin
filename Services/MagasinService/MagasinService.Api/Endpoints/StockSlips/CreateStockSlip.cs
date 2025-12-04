using IDR.Library.BuildingBlocks.CQRS;
using MagasinService.Application.Features.StockSlips.Commands.CreateStockSlip;
using MagasinService.Application.Features.StockSlips.DTOs;

namespace MagasinService.Api.Endpoints.StockSlips;

public class CreateStockSlip : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/stock-slips", async (CreateStockSlipRequest request, ISender sender) =>
        {
            var command = new CreateStockSlipCommand(request);
            var result = await sender.Send(command);

            return Results.Created($"/stock-slips/{result.Id}", result);
        })
        .WithName("CreateStockSlip")
        .Produces<StockSlipDto>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status409Conflict)
        .WithSummary("Créer un nouveau bordereau de transfert")
        .WithDescription("Crée un nouveau bordereau de transfert entre magasins");
    }
}