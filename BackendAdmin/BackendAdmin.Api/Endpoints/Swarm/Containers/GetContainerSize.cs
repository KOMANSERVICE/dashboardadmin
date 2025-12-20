using BackendAdmin.Application.Features.Swarm.DTOs;
using BackendAdmin.Application.Features.Swarm.Queries.GetContainerSize;

namespace BackendAdmin.Api.Endpoints.Swarm.Containers;

public record GetContainerSizeResponse(ContainerSizeDTO Size);

public class GetContainerSize : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/swarm/containers/{id}/size", async (string id, ISender sender) =>
        {
            var query = new GetContainerSizeQuery(id);
            var result = await sender.Send(query);
            var response = result.Adapt<GetContainerSizeResponse>();
            var baseResponse = ResponseFactory.Success(
                response,
                "Taille du conteneur recuperee avec succes",
                StatusCodes.Status200OK);

            return Results.Ok(baseResponse);
        })
        .WithName("GetContainerSize")
        .WithTags("Swarm", "Containers")
        .Produces<BaseResponse<GetContainerSizeResponse>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status500InternalServerError)
        .WithSummary("Recupere la taille d'un conteneur")
        .WithDescription("Retourne l'espace disque utilise par un conteneur Docker (SizeRw: couche ecriture, SizeRootFs: taille totale)")
        .RequireAuthorization();
    }
}
