using BackendAdmin.Application.Features.Swarm.Commands.RemoveNode;

namespace BackendAdmin.Api.Endpoints.Swarm;

public record RemoveNodeResponse(string Message);

public class RemoveNode : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("/api/swarm/nodes/{id}", async (string id, bool? force, ISender sender) =>
        {
            var command = new RemoveNodeCommand(id, force ?? false);
            var result = await sender.Send(command);
            var response = result.Adapt<RemoveNodeResponse>();
            var baseResponse = ResponseFactory.Success(
                response,
                "Noeud supprime du cluster avec succes",
                StatusCodes.Status200OK);

            return Results.Ok(baseResponse);
        })
        .WithName("RemoveSwarmNode")
        .WithTags("Swarm")
        .Produces<BaseResponse<RemoveNodeResponse>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status500InternalServerError)
        .WithSummary("Supprimer un noeud du cluster")
        .WithDescription("Retire un noeud du cluster Docker Swarm. Le noeud doit etre en mode drain sauf si force=true.")
        .RequireAuthorization();
    }
}
