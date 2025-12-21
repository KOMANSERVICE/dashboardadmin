using BackendAdmin.Application.Features.Swarm.DTOs;
using BackendAdmin.Application.Features.Swarm.Commands.CreateVolume;

namespace BackendAdmin.Api.Endpoints.Swarm;

public record CreateVolumeResponse(string VolumeName);

public class CreateVolume : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/swarm/volumes", async (CreateVolumeRequest request, ISender sender) =>
        {
            var command = new CreateVolumeCommand(
                Name: request.Name,
                Driver: request.Driver,
                Labels: request.Labels,
                DriverOpts: request.DriverOpts
            );
            var result = await sender.Send(command);
            var response = result.Adapt<CreateVolumeResponse>();
            var baseResponse = ResponseFactory.Success(
                response,
                $"Volume '{request.Name}' cree avec succes",
                StatusCodes.Status201Created);

            return Results.Created($"/api/swarm/volumes/{request.Name}", baseResponse);
        })
        .WithName("CreateVolume")
        .WithTags("Swarm", "Volumes")
        .Produces<BaseResponse<CreateVolumeResponse>>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status409Conflict)
        .ProducesProblem(StatusCodes.Status500InternalServerError)
        .WithSummary("Cree un nouveau volume Docker")
        .WithDescription("Cree un nouveau volume Docker avec le driver et les options specifies")
        .RequireAuthorization();
    }
}
