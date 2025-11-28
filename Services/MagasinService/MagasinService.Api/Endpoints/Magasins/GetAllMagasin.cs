using MagasinService.Application.Features.Magasins.DTOs;
using MagasinService.Application.Features.Magasins.Queries.GetAllMagasin;

namespace MagasinService.Api.Endpoints.Magasins;


public record GetAllMagasinResponse(List<StockLocationDTO> StockLocations);
public class GetAllMagasin : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/magasin/{BoutiqueId}", async (Guid BoutiqueId, ISender sender) =>
        {
            var command = new GetAllMagasinQuery(BoutiqueId);

            var result = await sender.Send(command);

            var response = result.Adapt<GetAllMagasinResponse>();
            var baseResponse = ResponseFactory.Success(response, "Liste des magasin réccupéré avec succèss", StatusCodes.Status200OK);

            return Results.Ok(baseResponse);
        })
       .WithName("GetAllMagasin")
       .WithTags("Magasin")
       .Produces<BaseResponse<GetAllMagasinResponse>>(StatusCodes.Status200OK)
       .ProducesProblem(StatusCodes.Status400BadRequest)
       .WithSummary("GetAllMagasin")
       .WithDescription("GetAllMagasin");
        //.RequireAuthorization();
    }
}
