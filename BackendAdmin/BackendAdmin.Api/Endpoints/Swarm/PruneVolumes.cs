using BackendAdmin.Application.Features.Swarm.DTOs;
using BackendAdmin.Application.Features.Swarm.Commands.PruneVolumes;

namespace BackendAdmin.Api.Endpoints.Swarm;

public record PruneVolumesApiResponse(
    int DeletedCount,
    long SpaceReclaimed,
    List<string> DeletedVolumes
);

public class PruneVolumes : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/swarm/volumes/prune", async (ISender sender) =>
        {
            var command = new PruneVolumesCommand();
            var result = await sender.Send(command);
            var response = result.Adapt<PruneVolumesApiResponse>();
            var baseResponse = ResponseFactory.Success(
                response,
                $"{result.DeletedCount} volume(s) supprime(s), {FormatBytes(result.SpaceReclaimed)} recupere(s)",
                StatusCodes.Status200OK);

            return Results.Ok(baseResponse);
        })
        .WithName("PruneVolumes")
        .WithTags("Swarm", "Volumes")
        .Produces<BaseResponse<PruneVolumesApiResponse>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status500InternalServerError)
        .WithSummary("Supprime les volumes inutilises")
        .WithDescription("Supprime tous les volumes Docker qui ne sont utilises par aucun container et retourne l'espace disque recupere")
        .RequireAuthorization();
    }

    private static string FormatBytes(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB", "TB" };
        int order = 0;
        double size = bytes;
        while (size >= 1024 && order < sizes.Length - 1)
        {
            order++;
            size /= 1024;
        }
        return $"{size:0.##} {sizes[order]}";
    }
}
