using BackendAdmin.Application.Features.Swarm.Commands.ActivateNode;

namespace BackendAdmin.Api.Endpoints.Swarm;

public record ActivateNodeResponse(string Message);

public class ActivateNode : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/swarm/nodes/{id}/activate", async (string id, ISender sender) =>
        {
            var command = new ActivateNodeCommand(id);
            var result = await sender.Send(command);
            var response = result.Adapt<ActivateNodeResponse>();
            var baseResponse = ResponseFactory.Success(
                response,
                "Noeud active avec succes",
                StatusCodes.Status200OK);

            return Results.Ok(baseResponse);
        })
        .WithName("ActivateSwarmNode")
        .WithTags("Swarm")
        .Produces<BaseResponse<ActivateNodeResponse>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status500InternalServerError)
        .WithSummary("Activer un noeud")
        .WithDescription("Reactive un noeud en mode drain ou pause pour qu'il puisse recevoir de nouvelles taches")
        .RequireAuthorization();
    }
}
