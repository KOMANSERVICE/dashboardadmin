using BackendAdmin.Application.Features.Swarm.DTOs;
using BackendAdmin.Application.Features.Swarm.Queries.GetImageDetails;

namespace BackendAdmin.Api.Endpoints.Swarm;

public record GetImageDetailsResponse(ImageDetailsDTO Image);

public class GetImageDetails : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/swarm/images/{id}", async (string id, ISender sender) =>
        {
            var query = new GetImageDetailsQuery(id);
            var result = await sender.Send(query);
            var response = result.Adapt<GetImageDetailsResponse>();
            var baseResponse = ResponseFactory.Success(
                response,
                "Details de l'image recuperes avec succes",
                StatusCodes.Status200OK);

            return Results.Ok(baseResponse);
        })
        .WithName("GetImageDetails")
        .WithTags("Swarm", "Images")
        .Produces<BaseResponse<GetImageDetailsResponse>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status500InternalServerError)
        .WithSummary("Recupere les details d'une image Docker")
        .WithDescription("Retourne les informations detaillees d'une image Docker (configuration, labels, layers, etc.)")
        .RequireAuthorization();
    }
}
