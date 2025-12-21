using BackendAdmin.Application.Features.Swarm.DTOs;
using BackendAdmin.Application.Features.Swarm.Queries.GetNodeDetails;

namespace BackendAdmin.Api.Endpoints.Swarm;

public record GetNodeDetailsResponse(NodeDetailsDTO Node);

public class GetNodeDetails : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/swarm/nodes/{id}", async (string id, ISender sender) =>
        {
            var query = new GetNodeDetailsQuery(id);
            var result = await sender.Send(query);
            var response = result.Adapt<GetNodeDetailsResponse>();
            var baseResponse = ResponseFactory.Success(
                response,
                "Details du noeud recuperes avec succes",
                StatusCodes.Status200OK);

            return Results.Ok(baseResponse);
        })
        .WithName("GetSwarmNodeDetails")
        .WithTags("Swarm")
        .Produces<BaseResponse<GetNodeDetailsResponse>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status500InternalServerError)
        .WithSummary("Recupere les details d'un noeud")
        .WithDescription("Retourne les informations detaillees d'un noeud Docker Swarm (ressources, role, etat, labels)")
        .RequireAuthorization();
    }
}
