using BackendAdmin.Application.Features.Swarm.DTOs;
using BackendAdmin.Application.Features.Swarm.Queries.GetStacks;

namespace BackendAdmin.Api.Endpoints.Swarm;

public record GetStacksResponse(List<StackDTO> Stacks);

public class GetStacks : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/swarm/stacks", async (ISender sender) =>
        {
            var query = new GetStacksQuery();
            var result = await sender.Send(query);
            var response = result.Adapt<GetStacksResponse>();
            var baseResponse = ResponseFactory.Success(
                response,
                "Liste des stacks Docker recuperee avec succes",
                StatusCodes.Status200OK);

            return Results.Ok(baseResponse);
        })
        .WithName("GetStacks")
        .WithTags("Swarm", "Stacks")
        .Produces<BaseResponse<GetStacksResponse>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status500InternalServerError)
        .WithSummary("Liste toutes les stacks Docker Swarm")
        .WithDescription("Retourne la liste de toutes les stacks deployees sur le cluster Docker Swarm avec le nombre de services pour chaque stack.")
        .RequireAuthorization();
    }
}
