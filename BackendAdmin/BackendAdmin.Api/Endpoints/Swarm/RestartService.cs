using BackendAdmin.Application.Features.Swarm.Commands.RestartService;

namespace BackendAdmin.Api.Endpoints.Swarm;

public record RestartServiceResponse(string Message);

public class RestartService : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/swarm/services/{name}/restart", async (string name, ISender sender) =>
        {
            var command = new RestartServiceCommand(name);
            var result = await sender.Send(command);
            var response = result.Adapt<RestartServiceResponse>();
            var baseResponse = ResponseFactory.Success(
                response,
                "Service redemarre avec succes",
                StatusCodes.Status200OK);

            return Results.Ok(baseResponse);
        })
        .WithName("RestartService")
        .WithTags("Swarm")
        .Produces<BaseResponse<RestartServiceResponse>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status500InternalServerError)
        .WithSummary("Redemarre un service")
        .WithDescription("Force le redemarrage d'un service Docker Swarm en incrementant le ForceUpdate")
        .RequireAuthorization();
    }
}
