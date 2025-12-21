using BackendAdmin.Application.Features.Swarm.DTOs;
using BackendAdmin.Application.Features.Swarm.Queries.GetImages;

namespace BackendAdmin.Api.Endpoints.Swarm;

public record GetImagesResponse(List<ImageDTO> Images);

public class GetImages : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/swarm/images", async (bool? all, ISender sender) =>
        {
            var query = new GetImagesQuery(all ?? false);
            var result = await sender.Send(query);
            var response = result.Adapt<GetImagesResponse>();
            var baseResponse = ResponseFactory.Success(
                response,
                "Liste des images Docker recuperee avec succes",
                StatusCodes.Status200OK);

            return Results.Ok(baseResponse);
        })
        .WithName("GetImages")
        .WithTags("Swarm", "Images")
        .Produces<BaseResponse<GetImagesResponse>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status500InternalServerError)
        .WithSummary("Liste toutes les images Docker")
        .WithDescription("Retourne la liste de toutes les images Docker avec leurs informations (taille, repository, tag, etc.)")
        .RequireAuthorization();
    }
}
