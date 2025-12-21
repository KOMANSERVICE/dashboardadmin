using BackendAdmin.Application.Features.Swarm.Commands.PromoteNode;

namespace BackendAdmin.Api.Endpoints.Swarm;

public record PromoteNodeResponse(string Message);

public class PromoteNode : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/swarm/nodes/{id}/promote", async (string id, ISender sender) =>
        {
            var command = new PromoteNodeCommand(id);
            var result = await sender.Send(command);
            var response = result.Adapt<PromoteNodeResponse>();
            var baseResponse = ResponseFactory.Success(
                response,
                "Noeud promu en manager avec succes",
                StatusCodes.Status200OK);

            return Results.Ok(baseResponse);
        })
        .WithName("PromoteSwarmNode")
        .WithTags("Swarm")
        .Produces<BaseResponse<PromoteNodeResponse>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status500InternalServerError)
        .WithSummary("Promouvoir un noeud en manager")
        .WithDescription("Promouvoir un worker en manager dans le cluster Docker Swarm")
        .RequireAuthorization();
    }
}
