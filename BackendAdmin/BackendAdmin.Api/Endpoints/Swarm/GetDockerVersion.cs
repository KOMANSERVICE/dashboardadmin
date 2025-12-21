using BackendAdmin.Application.Features.Swarm.DTOs;
using BackendAdmin.Application.Features.Swarm.Queries.GetDockerVersion;

namespace BackendAdmin.Api.Endpoints.Swarm;

public record GetDockerVersionResponse(DockerVersionDTO Version);

public class GetDockerVersion : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/swarm/system/version", async (ISender sender) =>
        {
            var query = new GetDockerVersionQuery();
            var result = await sender.Send(query);
            var response = result.Adapt<GetDockerVersionResponse>();
            var baseResponse = ResponseFactory.Success(
                response,
                "Version Docker recuperee avec succes",
                StatusCodes.Status200OK);

            return Results.Ok(baseResponse);
        })
        .WithName("GetDockerVersion")
        .WithTags("Swarm", "System")
        .Produces<BaseResponse<GetDockerVersionResponse>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status500InternalServerError)
        .WithSummary("Recupere la version de Docker Engine")
        .WithDescription("Retourne les informations de version de Docker Engine: version, API version, Git commit, Go version, etc.")
        .RequireAuthorization();
    }
}
