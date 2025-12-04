using IDR.Library.BuildingBlocks.CQRS;
using MagasinService.Application.Features.StockSlips.DTOs;
using MagasinService.Application.Features.StockSlips.Queries.GetStockSlips;

namespace MagasinService.Api.Endpoints.StockSlips;

public class GetStockSlipsByBoutique : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/stock-slips/boutique/{boutiqueId:guid}", async (Guid boutiqueId, ISender sender) =>
        {
            var query = new GetStockSlipsByBoutiqueQuery(boutiqueId);
            var result = await sender.Send(query);

            return Results.Ok(result);
        })
        .WithName("GetStockSlipsByBoutique")
        .Produces<IReadOnlyList<StockSlipDto>>(StatusCodes.Status200OK)
        .WithSummary("Récupérer les bordereaux d'une boutique")
        .WithDescription("Récupère tous les bordereaux de transfert liés à une boutique spécifique");
    }
}