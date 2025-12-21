using BackendAdmin.Application.Features.Swarm.Commands.DemoteNode;

namespace BackendAdmin.Api.Endpoints.Swarm;

public record DemoteNodeResponse(string Message);

public class DemoteNode : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/swarm/nodes/{id}/demote", async (string id, ISender sender) =>
        {
            var command = new DemoteNodeCommand(id);
            var result = await sender.Send(command);
            var response = result.Adapt<DemoteNodeResponse>();
            var baseResponse = ResponseFactory.Success(
                response,
                "Noeud retrograde en worker avec succes",
                StatusCodes.Status200OK);

            return Results.Ok(baseResponse);
        })
        .WithName("DemoteSwarmNode")
        .WithTags("Swarm")
        .Produces<BaseResponse<DemoteNodeResponse>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status500InternalServerError)
        .WithSummary("Retrograder un noeud en worker")
        .WithDescription("Retrograder un manager en worker dans le cluster Docker Swarm. Impossible si c'est le dernier manager.")
        .RequireAuthorization();
    }
}
