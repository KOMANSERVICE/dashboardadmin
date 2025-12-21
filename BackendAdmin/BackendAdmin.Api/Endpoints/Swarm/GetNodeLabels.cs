using BackendAdmin.Application.Features.Swarm.Queries.GetNodeLabels;

namespace BackendAdmin.Api.Endpoints.Swarm;

public record GetNodeLabelsResponse(string NodeId, Dictionary<string, string> Labels);

public class GetNodeLabels : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/swarm/nodes/{id}/labels", async (string id, ISender sender) =>
        {
            var query = new GetNodeLabelsQuery(id);
            var result = await sender.Send(query);
            var response = result.Adapt<GetNodeLabelsResponse>();
            var baseResponse = ResponseFactory.Success(
                response,
                "Labels du noeud recuperes avec succes",
                StatusCodes.Status200OK);

            return Results.Ok(baseResponse);
        })
        .WithName("GetSwarmNodeLabels")
        .WithTags("Swarm")
        .Produces<BaseResponse<GetNodeLabelsResponse>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status500InternalServerError)
        .WithSummary("Recupere les labels d'un noeud")
        .WithDescription("Retourne la liste des labels attaches a un noeud Docker Swarm")
        .RequireAuthorization();
    }
}
