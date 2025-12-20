using BackendAdmin.Application.Features.Swarm.DTOs;
using BackendAdmin.Application.Features.Swarm.Queries.GetContainerTop;

namespace BackendAdmin.Api.Endpoints.Swarm.Containers;

public record GetContainerTopResponse(ContainerTopDTO Top);

public class GetContainerTop : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/swarm/containers/{id}/top", async (string id, ISender sender) =>
        {
            var query = new GetContainerTopQuery(id);
            var result = await sender.Send(query);
            var response = result.Adapt<GetContainerTopResponse>();
            var baseResponse = ResponseFactory.Success(
                response,
                "Processus du conteneur recuperes avec succes",
                StatusCodes.Status200OK);

            return Results.Ok(baseResponse);
        })
        .WithName("GetContainerTop")
        .WithTags("Swarm", "Containers")
        .Produces<BaseResponse<GetContainerTopResponse>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status500InternalServerError)
        .WithSummary("Liste les processus d'un conteneur")
        .WithDescription("Retourne la liste des processus en cours d'execution dans un conteneur Docker (equivalent a docker top)")
        .RequireAuthorization();
    }
}
