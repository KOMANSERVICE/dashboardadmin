using BackendAdmin.Application.Features.Swarm.DTOs;
using BackendAdmin.Application.Features.Swarm.Queries.GetStackDetails;

namespace BackendAdmin.Api.Endpoints.Swarm;

public record GetStackDetailsResponse(StackDetailsDTO Stack);

public class GetStackDetails : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/swarm/stacks/{name}", async (string name, ISender sender) =>
        {
            var query = new GetStackDetailsQuery(name);
            var result = await sender.Send(query);
            var response = new GetStackDetailsResponse(result.Stack!);
            var baseResponse = ResponseFactory.Success(
                response,
                $"Details de la stack '{name}' recuperes avec succes",
                StatusCodes.Status200OK);

            return Results.Ok(baseResponse);
        })
        .WithName("GetStackDetails")
        .WithTags("Swarm", "Stacks")
        .Produces<BaseResponse<GetStackDetailsResponse>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status500InternalServerError)
        .WithSummary("Recupere les details d'une stack")
        .WithDescription("Retourne les details complets d'une stack Docker Swarm incluant la liste de tous ses services.")
        .RequireAuthorization();
    }
}
