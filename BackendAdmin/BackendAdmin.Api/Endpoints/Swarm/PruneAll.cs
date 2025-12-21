using BackendAdmin.Application.Features.Swarm.DTOs;
using BackendAdmin.Application.Features.Swarm.Commands.PruneAll;

namespace BackendAdmin.Api.Endpoints.Swarm;

public record PruneAllApiResponse(
    int ContainersDeleted,
    long ContainersSpaceReclaimed,
    int ImagesDeleted,
    long ImagesSpaceReclaimed,
    int VolumesDeleted,
    long VolumesSpaceReclaimed,
    int NetworksDeleted,
    int BuildCacheDeleted,
    long BuildCacheSpaceReclaimed,
    long TotalSpaceReclaimed,
    List<string> DeletedContainers,
    List<string> DeletedImages,
    List<string> DeletedVolumes,
    List<string> DeletedNetworks
);

public class PruneAll : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/swarm/system/prune", async (ISender sender) =>
        {
            var command = new PruneAllCommand();
            var result = await sender.Send(command);
            var response = new PruneAllApiResponse(
                ContainersDeleted: result.Response.ContainersDeleted,
                ContainersSpaceReclaimed: result.Response.ContainersSpaceReclaimed,
                ImagesDeleted: result.Response.ImagesDeleted,
                ImagesSpaceReclaimed: result.Response.ImagesSpaceReclaimed,
                VolumesDeleted: result.Response.VolumesDeleted,
                VolumesSpaceReclaimed: result.Response.VolumesSpaceReclaimed,
                NetworksDeleted: result.Response.NetworksDeleted,
                BuildCacheDeleted: result.Response.BuildCacheDeleted,
                BuildCacheSpaceReclaimed: result.Response.BuildCacheSpaceReclaimed,
                TotalSpaceReclaimed: result.Response.TotalSpaceReclaimed,
                DeletedContainers: result.Response.DeletedContainers,
                DeletedImages: result.Response.DeletedImages,
                DeletedVolumes: result.Response.DeletedVolumes,
                DeletedNetworks: result.Response.DeletedNetworks
            );

            var baseResponse = ResponseFactory.Success(
                response,
                $"Nettoyage termine: {FormatBytes(result.Response.TotalSpaceReclaimed)} recupere(s)",
                StatusCodes.Status200OK);

            return Results.Ok(baseResponse);
        })
        .WithName("PruneAll")
        .WithTags("Swarm", "System")
        .Produces<BaseResponse<PruneAllApiResponse>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status500InternalServerError)
        .WithSummary("Nettoie toutes les ressources Docker inutilisees")
        .WithDescription("Supprime tous les conteneurs arretes, images non utilisees, volumes orphelins, reseaux inutilises et le cache de build. Retourne l'espace disque total recupere.")
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
