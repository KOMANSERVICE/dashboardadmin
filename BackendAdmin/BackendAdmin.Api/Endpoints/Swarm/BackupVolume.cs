using BackendAdmin.Application.Features.Swarm.DTOs;
using BackendAdmin.Application.Features.Swarm.Commands.BackupVolume;

namespace BackendAdmin.Api.Endpoints.Swarm;

public record BackupVolumeApiResponse(
    string VolumeName,
    string BackupPath,
    DateTime BackupDate,
    long SizeBytes
);

public class BackupVolume : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/swarm/volumes/{name}/backup", async (string name, BackupVolumeRequest request, ISender sender) =>
        {
            var command = new BackupVolumeCommand(name, request.DestinationPath);
            var result = await sender.Send(command);
            var response = result.Adapt<BackupVolumeApiResponse>();
            var baseResponse = ResponseFactory.Success(
                response,
                $"Backup du volume '{name}' cree avec succes",
                StatusCodes.Status200OK);

            return Results.Ok(baseResponse);
        })
        .WithName("BackupVolume")
        .WithTags("Swarm", "Volumes")
        .Produces<BaseResponse<BackupVolumeApiResponse>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status500InternalServerError)
        .WithSummary("Backup un volume Docker")
        .WithDescription("Cree une archive tar du contenu d'un volume Docker vers le chemin de destination specifie")
        .RequireAuthorization();
    }
}
