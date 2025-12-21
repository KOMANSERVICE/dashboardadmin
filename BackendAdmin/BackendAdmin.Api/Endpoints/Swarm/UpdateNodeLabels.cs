using BackendAdmin.Application.Features.Swarm.Commands.UpdateNodeLabels;

namespace BackendAdmin.Api.Endpoints.Swarm;

public record UpdateNodeLabelsRequest(Dictionary<string, string> Labels);
public record UpdateNodeLabelsResponse(string Message);

public class UpdateNodeLabels : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("/api/swarm/nodes/{id}/labels", async (string id, UpdateNodeLabelsRequest request, ISender sender) =>
        {
            var command = new UpdateNodeLabelsCommand(id, request.Labels);
            var result = await sender.Send(command);
            var response = result.Adapt<UpdateNodeLabelsResponse>();
            var baseResponse = ResponseFactory.Success(
                response,
                "Labels du noeud mis a jour avec succes",
                StatusCodes.Status200OK);

            return Results.Ok(baseResponse);
        })
        .WithName("UpdateSwarmNodeLabels")
        .WithTags("Swarm")
        .Produces<BaseResponse<UpdateNodeLabelsResponse>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status500InternalServerError)
        .WithSummary("Met a jour les labels d'un noeud")
        .WithDescription("Remplace les labels d'un noeud Docker Swarm par les nouveaux labels fournis")
        .RequireAuthorization();
    }
}
