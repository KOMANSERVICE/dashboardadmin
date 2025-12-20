using BackendAdmin.Application.Features.Swarm.DTOs;
using BackendAdmin.Application.Features.Swarm.Queries.GetNetworkDetails;

namespace BackendAdmin.Api.Endpoints.Swarm;

public record GetNetworkDetailsResponse(NetworkDetailsDTO Network);

public class GetNetworkDetails : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/swarm/networks/{name}", async (string name, ISender sender) =>
        {
            var query = new GetNetworkDetailsQuery(name);
            var result = await sender.Send(query);
            var response = result.Adapt<GetNetworkDetailsResponse>();
            var baseResponse = ResponseFactory.Success(
                response,
                $"Details du reseau '{name}' recuperes avec succes",
                StatusCodes.Status200OK);

            return Results.Ok(baseResponse);
        })
        .WithName("GetNetworkDetails")
        .WithTags("Swarm", "Networks")
        .Produces<BaseResponse<GetNetworkDetailsResponse>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status500InternalServerError)
        .WithSummary("Obtient les details d'un reseau Docker")
        .WithDescription("Retourne les informations detaillees d'un reseau Docker incluant sa configuration IPAM et les conteneurs connectes")
        .RequireAuthorization();
    }
}
