using BackendAdmin.Application.Features.Swarm.DTOs;
using BackendAdmin.Application.Features.Swarm.Queries.GetSwarmServices;

namespace BackendAdmin.Api.Endpoints.Swarm;

public record GetSwarmServicesResponse(List<SwarmServiceDTO> Services);

public class GetSwarmServices : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/swarm/services", async (ISender sender) =>
        {
            var query = new GetSwarmServicesQuery();
            var result = await sender.Send(query);
            var response = result.Adapt<GetSwarmServicesResponse>();
            var baseResponse = ResponseFactory.Success(
                response,
                "Liste des services Docker Swarm recuperee avec succes",
                StatusCodes.Status200OK);

            return Results.Ok(baseResponse);
        })
        .WithName("GetSwarmServices")
        .WithTags("Swarm")
        .Produces<BaseResponse<GetSwarmServicesResponse>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status500InternalServerError)
        .WithSummary("Liste tous les services Docker Swarm")
        .WithDescription("Retourne la liste de tous les services Docker Swarm avec leurs informations (replicas, image, status, etc.)")
        .RequireAuthorization();
    }
}
