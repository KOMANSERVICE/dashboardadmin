using BackendAdmin.Application.Features.Swarm.DTOs;
using BackendAdmin.Application.Features.Swarm.Queries.GetUnusedVolumes;

namespace BackendAdmin.Api.Endpoints.Swarm;

public record GetUnusedVolumesResponse(List<VolumeDTO> Volumes);

public class GetUnusedVolumes : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/swarm/volumes/unused", async (ISender sender) =>
        {
            var query = new GetUnusedVolumesQuery();
            var result = await sender.Send(query);
            var response = result.Adapt<GetUnusedVolumesResponse>();
            var baseResponse = ResponseFactory.Success(
                response,
                "Liste des volumes inutilises recuperee avec succes",
                StatusCodes.Status200OK);

            return Results.Ok(baseResponse);
        })
        .WithName("GetUnusedVolumes")
        .WithTags("Swarm", "Volumes")
        .Produces<BaseResponse<GetUnusedVolumesResponse>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status500InternalServerError)
        .WithSummary("Liste les volumes orphelins")
        .WithDescription("Retourne la liste des volumes Docker qui ne sont utilises par aucun container (running ou stopped)")
        .RequireAuthorization();
    }
}
