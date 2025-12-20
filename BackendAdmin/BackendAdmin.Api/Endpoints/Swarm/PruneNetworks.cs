using BackendAdmin.Application.Features.Swarm.Commands.PruneNetworks;

namespace BackendAdmin.Api.Endpoints.Swarm;

public record PruneNetworksApiResponse(
    int DeletedCount,
    List<string> DeletedNetworks
);

public class PruneNetworks : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/swarm/networks/prune", async (ISender sender) =>
        {
            var command = new PruneNetworksCommand();
            var result = await sender.Send(command);
            var response = result.Adapt<PruneNetworksApiResponse>();
            var baseResponse = ResponseFactory.Success(
                response,
                $"{result.DeletedCount} reseau(x) supprime(s)",
                StatusCodes.Status200OK);

            return Results.Ok(baseResponse);
        })
        .WithName("PruneNetworks")
        .WithTags("Swarm", "Networks")
        .Produces<BaseResponse<PruneNetworksApiResponse>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status500InternalServerError)
        .WithSummary("Supprime les reseaux inutilises")
        .WithDescription("Supprime tous les reseaux Docker qui ne sont connectes a aucun conteneur")
        .RequireAuthorization();
    }
}
