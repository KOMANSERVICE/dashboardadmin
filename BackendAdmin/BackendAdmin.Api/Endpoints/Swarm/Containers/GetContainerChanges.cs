using BackendAdmin.Application.Features.Swarm.DTOs;
using BackendAdmin.Application.Features.Swarm.Queries.GetContainerChanges;

namespace BackendAdmin.Api.Endpoints.Swarm.Containers;

public record GetContainerChangesResponse(ContainerChangesDTO Changes);

public class GetContainerChanges : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/swarm/containers/{id}/changes", async (string id, ISender sender) =>
        {
            var query = new GetContainerChangesQuery(id);
            var result = await sender.Send(query);
            var response = result.Adapt<GetContainerChangesResponse>();
            var baseResponse = ResponseFactory.Success(
                response,
                "Modifications du filesystem recuperees avec succes",
                StatusCodes.Status200OK);

            return Results.Ok(baseResponse);
        })
        .WithName("GetContainerChanges")
        .WithTags("Swarm", "Containers")
        .Produces<BaseResponse<GetContainerChangesResponse>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status500InternalServerError)
        .WithSummary("Liste les modifications du filesystem d'un conteneur")
        .WithDescription("Retourne la liste des fichiers modifies, ajoutes ou supprimes dans un conteneur Docker")
        .RequireAuthorization();
    }
}
