using BackendAdmin.Application.Features.Swarm.DTOs;
using BackendAdmin.Application.Features.Swarm.Queries.GetDockerEvents;

namespace BackendAdmin.Api.Endpoints.Swarm;

public record GetDockerEventsResponse(IList<DockerEventDTO> Events);

public class GetDockerEvents : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/swarm/events", async (ISender sender, DateTime? since, DateTime? until) =>
        {
            var query = new GetDockerEventsQuery(since, until);
            var result = await sender.Send(query);
            var response = result.Adapt<GetDockerEventsResponse>();
            var baseResponse = ResponseFactory.Success(
                response,
                "Evenements Docker recuperes avec succes",
                StatusCodes.Status200OK);

            return Results.Ok(baseResponse);
        })
        .WithName("GetDockerEvents")
        .WithTags("Swarm", "Events")
        .Produces<BaseResponse<GetDockerEventsResponse>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status500InternalServerError)
        .WithSummary("Recupere les evenements Docker")
        .WithDescription("Retourne les evenements Docker recents. Par defaut, retourne les evenements des 15 dernieres minutes. Utilisez les parametres 'since' et 'until' pour filtrer par periode.")
        .RequireAuthorization();
    }
}
