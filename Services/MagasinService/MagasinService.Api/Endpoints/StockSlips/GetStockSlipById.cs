using IDR.Library.BuildingBlocks.CQRS;
using MagasinService.Application.Features.StockSlips.DTOs;
using MagasinService.Application.Features.StockSlips.Queries.GetStockSlipById;

namespace MagasinService.Api.Endpoints.StockSlips;

public class GetStockSlipById : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/stock-slips/{id:guid}", async (Guid id, ISender sender) =>
        {
            var query = new GetStockSlipByIdQuery(id);
            var result = await sender.Send(query);

            return Results.Ok(result);
        })
        .WithName("GetStockSlipById")
        .Produces<StockSlipDto>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .WithSummary("Récupérer un bordereau par son ID")
        .WithDescription("Récupère un bordereau de transfert avec tous ses détails");
    }
}