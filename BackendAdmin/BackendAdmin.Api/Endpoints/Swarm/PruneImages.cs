using BackendAdmin.Application.Features.Swarm.Commands.PruneImages;

namespace BackendAdmin.Api.Endpoints.Swarm;

public record PruneImagesApiResponse(
    int DeletedCount,
    long SpaceReclaimed,
    List<string> DeletedImages
);

public class PruneImages : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/swarm/images/prune", async (bool? dangling, ISender sender) =>
        {
            var command = new PruneImagesCommand(dangling ?? true);
            var result = await sender.Send(command);
            var response = result.Adapt<PruneImagesApiResponse>();
            var baseResponse = ResponseFactory.Success(
                response,
                $"{result.DeletedCount} image(s) supprimee(s), {FormatBytes(result.SpaceReclaimed)} recupere(s)",
                StatusCodes.Status200OK);

            return Results.Ok(baseResponse);
        })
        .WithName("PruneImages")
        .WithTags("Swarm", "Images")
        .Produces<BaseResponse<PruneImagesApiResponse>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status500InternalServerError)
        .WithSummary("Supprime les images inutilisees")
        .WithDescription("Supprime toutes les images Docker non utilisees (dangling par defaut) et retourne l'espace disque recupere")
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
