using BackendAdmin.Application.Features.Swarm.DTOs;
using BackendAdmin.Application.Features.Swarm.Queries.GetServiceTasks;

namespace BackendAdmin.Api.Endpoints.Swarm;

public record GetServiceTasksResponse(List<ServiceTaskDTO> Tasks);

public class GetServiceTasks : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/swarm/services/{name}/tasks", async (string name, ISender sender) =>
        {
            var query = new GetServiceTasksQuery(name);
            var result = await sender.Send(query);
            var response = result.Adapt<GetServiceTasksResponse>();
            var baseResponse = ResponseFactory.Success(
                response,
                "Taches du service Docker Swarm recuperees avec succes",
                StatusCodes.Status200OK);

            return Results.Ok(baseResponse);
        })
        .WithName("GetSwarmServiceTasks")
        .WithTags("Swarm")
        .Produces<BaseResponse<GetServiceTasksResponse>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status500InternalServerError)
        .WithSummary("Liste les taches d'un service Docker Swarm")
        .WithDescription("Retourne la liste des taches (conteneurs) d'un service Docker Swarm avec leur etat (id, nodeId, state, containerId)")
        .RequireAuthorization();
    }
}
