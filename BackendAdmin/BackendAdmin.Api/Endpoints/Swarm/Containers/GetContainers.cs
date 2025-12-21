using BackendAdmin.Application.Features.Swarm.DTOs;
using BackendAdmin.Application.Features.Swarm.Queries.GetContainers;

namespace BackendAdmin.Api.Endpoints.Swarm.Containers;

public record GetContainersResponse(List<ContainerDTO> Containers);

public class GetContainers : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/swarm/containers", async (bool? all, ISender sender) =>
        {
            var query = new GetContainersQuery(all ?? true);
            var result = await sender.Send(query);
            var response = result.Adapt<GetContainersResponse>();
            var baseResponse = ResponseFactory.Success(
                response,
                "Liste des conteneurs Docker recuperee avec succes",
                StatusCodes.Status200OK);

            return Results.Ok(baseResponse);
        })
        .WithName("GetContainers")
        .WithTags("Swarm", "Containers")
        .Produces<BaseResponse<GetContainersResponse>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status500InternalServerError)
        .WithSummary("Liste tous les conteneurs Docker")
        .WithDescription("Retourne la liste de tous les conteneurs Docker. Parametre optionnel: all (true/false) pour inclure les conteneurs arretes")
        .RequireAuthorization();
    }
}
