using BackendAdmin.Application.Features.Swarm.DTOs;
using BackendAdmin.Application.Features.Swarm.Commands.RestoreVolume;

namespace BackendAdmin.Api.Endpoints.Swarm;

public record RestoreVolumeApiResponse(
    string VolumeName,
    string SourcePath,
    DateTime RestoreDate
);

public class RestoreVolume : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/swarm/volumes/{name}/restore", async (string name, RestoreVolumeRequest request, ISender sender) =>
        {
            var command = new RestoreVolumeCommand(name, request.SourcePath);
            var result = await sender.Send(command);
            var response = result.Adapt<RestoreVolumeApiResponse>();
            var baseResponse = ResponseFactory.Success(
                response,
                $"Volume '{name}' restaure avec succes depuis '{request.SourcePath}'",
                StatusCodes.Status200OK);

            return Results.Ok(baseResponse);
        })
        .WithName("RestoreVolume")
        .WithTags("Swarm", "Volumes")
        .Produces<BaseResponse<RestoreVolumeApiResponse>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status500InternalServerError)
        .WithSummary("Restore un volume Docker")
        .WithDescription("Restaure le contenu d'un volume Docker depuis une archive tar. Le volume sera cree s'il n'existe pas.")
        .RequireAuthorization();
    }
}
