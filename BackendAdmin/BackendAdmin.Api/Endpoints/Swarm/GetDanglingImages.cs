using BackendAdmin.Application.Features.Swarm.DTOs;
using BackendAdmin.Application.Features.Swarm.Queries.GetDanglingImages;

namespace BackendAdmin.Api.Endpoints.Swarm;

public record GetDanglingImagesResponse(List<ImageDTO> Images);

public class GetDanglingImages : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/swarm/images/dangling", async (ISender sender) =>
        {
            var query = new GetDanglingImagesQuery();
            var result = await sender.Send(query);
            var response = result.Adapt<GetDanglingImagesResponse>();
            var baseResponse = ResponseFactory.Success(
                response,
                "Liste des images dangling recuperee avec succes",
                StatusCodes.Status200OK);

            return Results.Ok(baseResponse);
        })
        .WithName("GetDanglingImages")
        .WithTags("Swarm", "Images")
        .Produces<BaseResponse<GetDanglingImagesResponse>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status500InternalServerError)
        .WithSummary("Liste les images Docker dangling")
        .WithDescription("Retourne la liste des images Docker sans tag (dangling) qui peuvent etre nettoyees")
        .RequireAuthorization();
    }
}
