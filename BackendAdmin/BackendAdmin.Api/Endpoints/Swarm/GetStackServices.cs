using BackendAdmin.Application.Features.Swarm.DTOs;
using BackendAdmin.Application.Features.Swarm.Queries.GetStackServices;

namespace BackendAdmin.Api.Endpoints.Swarm;

public record GetStackServicesResponse(List<StackServiceDTO> Services);

public class GetStackServices : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/swarm/stacks/{name}/services", async (string name, ISender sender) =>
        {
            var query = new GetStackServicesQuery(name);
            var result = await sender.Send(query);
            var response = result.Adapt<GetStackServicesResponse>();
            var baseResponse = ResponseFactory.Success(
                response,
                $"Services de la stack '{name}' recuperes avec succes",
                StatusCodes.Status200OK);

            return Results.Ok(baseResponse);
        })
        .WithName("GetStackServices")
        .WithTags("Swarm", "Stacks")
        .Produces<BaseResponse<GetStackServicesResponse>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status500InternalServerError)
        .WithSummary("Liste les services d'une stack")
        .WithDescription("Retourne la liste de tous les services appartenant a une stack Docker Swarm.")
        .RequireAuthorization();
    }
}
