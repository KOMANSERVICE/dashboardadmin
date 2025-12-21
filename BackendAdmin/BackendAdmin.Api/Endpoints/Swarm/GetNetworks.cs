using BackendAdmin.Application.Features.Swarm.DTOs;
using BackendAdmin.Application.Features.Swarm.Queries.GetNetworks;

namespace BackendAdmin.Api.Endpoints.Swarm;

public record GetNetworksResponse(List<NetworkDTO> Networks);

public class GetNetworks : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/swarm/networks", async (ISender sender) =>
        {
            var query = new GetNetworksQuery();
            var result = await sender.Send(query);
            var response = result.Adapt<GetNetworksResponse>();
            var baseResponse = ResponseFactory.Success(
                response,
                "Liste des reseaux Docker recuperee avec succes",
                StatusCodes.Status200OK);

            return Results.Ok(baseResponse);
        })
        .WithName("GetNetworks")
        .WithTags("Swarm", "Networks")
        .Produces<BaseResponse<GetNetworksResponse>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status500InternalServerError)
        .WithSummary("Liste tous les reseaux Docker")
        .WithDescription("Retourne la liste de tous les reseaux Docker avec leurs informations (driver, scope, conteneurs connectes, etc.)")
        .RequireAuthorization();
    }
}
