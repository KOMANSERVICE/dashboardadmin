using BackendAdmin.Application.Features.Swarm.DTOs;
using BackendAdmin.Application.Features.Swarm.Queries.GetServiceDetails;

namespace BackendAdmin.Api.Endpoints.Swarm;

public record GetServiceDetailsResponse(ServiceDetailsDTO Service);

public class GetServiceDetails : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/swarm/services/{name}", async (string name, ISender sender) =>
        {
            var query = new GetServiceDetailsQuery(name);
            var result = await sender.Send(query);
            var response = result.Adapt<GetServiceDetailsResponse>();
            var baseResponse = ResponseFactory.Success(
                response,
                "Details du service Docker Swarm recuperes avec succes",
                StatusCodes.Status200OK);

            return Results.Ok(baseResponse);
        })
        .WithName("GetSwarmServiceDetails")
        .WithTags("Swarm")
        .Produces<BaseResponse<GetServiceDetailsResponse>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status500InternalServerError)
        .WithSummary("Obtient les details d'un service Docker Swarm")
        .WithDescription("Retourne les informations detaillees d'un service Docker Swarm (config, replicas, ports, image, env, labels, networks)")
        .RequireAuthorization();
    }
}
