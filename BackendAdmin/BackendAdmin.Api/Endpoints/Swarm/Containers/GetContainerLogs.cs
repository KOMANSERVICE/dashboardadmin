using BackendAdmin.Application.Features.Swarm.DTOs;
using BackendAdmin.Application.Features.Swarm.Queries.GetContainerLogs;

namespace BackendAdmin.Api.Endpoints.Swarm.Containers;

public record GetContainerLogsResponse(ContainerLogsDTO Logs);

public class GetContainerLogs : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/swarm/containers/{id}/logs", async (string id, int? tail, bool? timestamps, ISender sender) =>
        {
            var query = new GetContainerLogsQuery(id, tail, timestamps ?? false);
            var result = await sender.Send(query);
            var response = result.Adapt<GetContainerLogsResponse>();
            var baseResponse = ResponseFactory.Success(
                response,
                "Logs du conteneur recuperes avec succes",
                StatusCodes.Status200OK);

            return Results.Ok(baseResponse);
        })
        .WithName("GetContainerLogs")
        .WithTags("Swarm", "Containers")
        .Produces<BaseResponse<GetContainerLogsResponse>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status500InternalServerError)
        .WithSummary("Recupere les logs d'un conteneur")
        .WithDescription("Retourne les logs stdout/stderr d'un conteneur Docker. Parametres optionnels: tail (nombre de lignes), timestamps (true/false)")
        .RequireAuthorization();
    }
}
