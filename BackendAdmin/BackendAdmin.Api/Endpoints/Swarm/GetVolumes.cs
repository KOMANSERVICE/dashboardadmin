using BackendAdmin.Application.Features.Swarm.DTOs;
using BackendAdmin.Application.Features.Swarm.Queries.GetVolumes;

namespace BackendAdmin.Api.Endpoints.Swarm;

public record GetVolumesResponse(List<VolumeDTO> Volumes);

public class GetVolumes : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/swarm/volumes", async (ISender sender) =>
        {
            var query = new GetVolumesQuery();
            var result = await sender.Send(query);
            var response = result.Adapt<GetVolumesResponse>();
            var baseResponse = ResponseFactory.Success(
                response,
                "Liste des volumes Docker recuperee avec succes",
                StatusCodes.Status200OK);

            return Results.Ok(baseResponse);
        })
        .WithName("GetVolumes")
        .WithTags("Swarm", "Volumes")
        .Produces<BaseResponse<GetVolumesResponse>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status500InternalServerError)
        .WithSummary("Liste tous les volumes Docker")
        .WithDescription("Retourne la liste de tous les volumes Docker avec leurs informations (taille, driver, mountpoint, etc.)")
        .RequireAuthorization();
    }
}
