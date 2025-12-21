using BackendAdmin.Application.Features.Swarm.DTOs;
using BackendAdmin.Application.Features.Swarm.Queries.GetContainerInspect;

namespace BackendAdmin.Api.Endpoints.Swarm.Containers;

public record GetContainerInspectResponse(ContainerDetailsDTO Details);

public class GetContainerInspect : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/swarm/containers/{id}/inspect", async (string id, ISender sender) =>
        {
            var query = new GetContainerInspectQuery(id);
            var result = await sender.Send(query);
            var response = result.Adapt<GetContainerInspectResponse>();
            var baseResponse = ResponseFactory.Success(
                response,
                "Configuration du conteneur recuperee avec succes",
                StatusCodes.Status200OK);

            return Results.Ok(baseResponse);
        })
        .WithName("GetContainerInspect")
        .WithTags("Swarm", "Containers")
        .Produces<BaseResponse<GetContainerInspectResponse>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status500InternalServerError)
        .WithSummary("Inspecte un conteneur")
        .WithDescription("Retourne la configuration complete d'un conteneur Docker (volumes, reseaux, variables d'environnement, etc.)")
        .RequireAuthorization();
    }
}
