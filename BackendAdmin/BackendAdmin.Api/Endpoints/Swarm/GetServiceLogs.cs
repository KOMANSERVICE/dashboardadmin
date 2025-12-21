using BackendAdmin.Application.Features.Swarm.DTOs;
using BackendAdmin.Application.Features.Swarm.Queries.GetServiceLogs;

namespace BackendAdmin.Api.Endpoints.Swarm;

public record GetServiceLogsResponse(ServiceLogsDTO Logs);

public class GetServiceLogs : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/swarm/services/{name}/logs", async (string name, int? tail, string? since, ISender sender) =>
        {
            var query = new GetServiceLogsQuery(name, tail, since);
            var result = await sender.Send(query);
            var response = result.Adapt<GetServiceLogsResponse>();
            var baseResponse = ResponseFactory.Success(
                response,
                "Logs du service recuperes avec succes",
                StatusCodes.Status200OK);

            return Results.Ok(baseResponse);
        })
        .WithName("GetServiceLogs")
        .WithTags("Swarm")
        .Produces<BaseResponse<GetServiceLogsResponse>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status500InternalServerError)
        .WithSummary("Recupere les logs d'un service")
        .WithDescription("Retourne les logs d'un service Docker Swarm. Parametres optionnels: tail (nombre de lignes), since (depuis quand, ex: 1h)")
        .RequireAuthorization();
    }
}
