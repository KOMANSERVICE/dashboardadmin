using MagasinService.Application.Features.Magasins.DTOs;
using MagasinService.Application.Features.Magasins.Queries.GetOneMagasin;
using Microsoft.AspNetCore.Mvc;

namespace MagasinService.Api.Endpoints.Magasins;

public record GetOneMagasinResponse(StockLocationDTO StockLocation);

public class GetOneMagasin : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/magasin/{BoutiqueId:guid}/{id:guid}", async (Guid BoutiqueId, Guid id, ISender sender) =>
        {
            var query = new GetOneMagasinQuery(BoutiqueId, id);

            var result = await sender.Send(query);

            var response = result.Adapt<GetOneMagasinResponse>();
            var baseResponse = ResponseFactory.Success(response, "Magasin récupéré avec succès", StatusCodes.Status200OK);

            return Results.Ok(baseResponse);
        })
       .WithName("GetOneMagasin")
       .WithTags("Magasin")
       .Produces<BaseResponse<GetOneMagasinResponse>>(StatusCodes.Status200OK)
       .Produces<ProblemDetails>(StatusCodes.Status404NotFound)
       .ProducesProblem(StatusCodes.Status400BadRequest)
       .WithSummary("Récupérer un magasin par son ID")
       .WithDescription("Permet de récupérer les détails d'un magasin spécifique par son identifiant unique");
        //.RequireAuthorization();
    }
}
