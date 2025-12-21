using BackendAdmin.Application.Features.Swarm.DTOs;
using BackendAdmin.Application.Features.Swarm.Queries.GetServiceResources;

namespace BackendAdmin.Api.Endpoints.Swarm;

public record GetServiceResourcesResponse(ServiceResourcesDTO Resources);

public class GetServiceResources : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/swarm/services/{name}/resources", async (string name, ISender sender) =>
        {
            var query = new GetServiceResourcesQuery(name);
            var result = await sender.Send(query);
            var response = new GetServiceResourcesResponse(result.Resources);
            var baseResponse = ResponseFactory.Success(
                response,
                $"Ressources du service '{name}' recuperees avec succes",
                StatusCodes.Status200OK);

            return Results.Ok(baseResponse);
        })
        .WithName("GetServiceResources")
        .WithTags("Swarm")
        .Produces<BaseResponse<GetServiceResourcesResponse>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status500InternalServerError)
        .WithSummary("Recupere les ressources et limites d'un service Docker Swarm")
        .WithDescription("Retourne les limites CPU, memoire, PIDs et ulimits configures pour un service")
        .RequireAuthorization();
    }
}
