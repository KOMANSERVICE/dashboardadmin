using BackendAdmin.Application.Features.Swarm.Commands.DrainNode;

namespace BackendAdmin.Api.Endpoints.Swarm;

public record DrainNodeResponse(string Message);

public class DrainNode : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/swarm/nodes/{id}/drain", async (string id, ISender sender) =>
        {
            var command = new DrainNodeCommand(id);
            var result = await sender.Send(command);
            var response = result.Adapt<DrainNodeResponse>();
            var baseResponse = ResponseFactory.Success(
                response,
                "Noeud mis en mode drain avec succes",
                StatusCodes.Status200OK);

            return Results.Ok(baseResponse);
        })
        .WithName("DrainSwarmNode")
        .WithTags("Swarm")
        .Produces<BaseResponse<DrainNodeResponse>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status500InternalServerError)
        .WithSummary("Mettre un noeud en mode drain")
        .WithDescription("Evacue les taches du noeud et empeche le scheduling de nouvelles taches")
        .RequireAuthorization();
    }
}
