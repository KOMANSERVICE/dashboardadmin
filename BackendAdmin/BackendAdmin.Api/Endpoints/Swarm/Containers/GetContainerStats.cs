using BackendAdmin.Application.Features.Swarm.DTOs;
using BackendAdmin.Application.Features.Swarm.Queries.GetContainerStats;

namespace BackendAdmin.Api.Endpoints.Swarm.Containers;

public record GetContainerStatsResponse(ContainerStatsDTO Stats);

public class GetContainerStats : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/swarm/containers/{id}/stats", async (string id, ISender sender) =>
        {
            var query = new GetContainerStatsQuery(id);
            var result = await sender.Send(query);
            var response = result.Adapt<GetContainerStatsResponse>();
            var baseResponse = ResponseFactory.Success(
                response,
                "Statistiques du conteneur recuperees avec succes",
                StatusCodes.Status200OK);

            return Results.Ok(baseResponse);
        })
        .WithName("GetContainerStats")
        .WithTags("Swarm", "Containers")
        .Produces<BaseResponse<GetContainerStatsResponse>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status500InternalServerError)
        .WithSummary("Recupere les statistiques d'un conteneur")
        .WithDescription("Retourne les statistiques CPU, memoire, reseau et I/O d'un conteneur Docker")
        .RequireAuthorization();
    }
}
