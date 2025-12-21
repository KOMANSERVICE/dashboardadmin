using BackendAdmin.Application.Features.Swarm.DTOs;
using BackendAdmin.Application.Features.Swarm.Queries.GetNodes;

namespace BackendAdmin.Api.Endpoints.Swarm;

public record GetNodesResponse(List<NodeDTO> Nodes);

public class GetNodes : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/swarm/nodes", async (ISender sender) =>
        {
            var query = new GetNodesQuery();
            var result = await sender.Send(query);
            var response = result.Adapt<GetNodesResponse>();
            var baseResponse = ResponseFactory.Success(
                response,
                "Liste des noeuds Docker Swarm recuperee avec succes",
                StatusCodes.Status200OK);

            return Results.Ok(baseResponse);
        })
        .WithName("GetSwarmNodes")
        .WithTags("Swarm")
        .Produces<BaseResponse<GetNodesResponse>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status500InternalServerError)
        .WithSummary("Liste tous les noeuds du cluster Docker Swarm")
        .WithDescription("Retourne la liste de tous les noeuds du cluster Docker Swarm avec leurs informations (hostname, role, state, resources, etc.)")
        .RequireAuthorization();
    }
}
