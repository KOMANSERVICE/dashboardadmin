using BackendAdmin.Application.Features.Swarm.Commands.RollbackService;

namespace BackendAdmin.Api.Endpoints.Swarm;

public record RollbackServiceResponse(string ServiceName);

public class RollbackService : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/swarm/services/{name}/rollback", async (string name, ISender sender) =>
        {
            var command = new RollbackServiceCommand(name);
            var result = await sender.Send(command);
            var response = result.Adapt<RollbackServiceResponse>();
            var baseResponse = ResponseFactory.Success(
                response,
                $"Service '{name}' rollback effectue avec succes",
                StatusCodes.Status200OK);

            return Results.Ok(baseResponse);
        })
        .WithName("RollbackSwarmService")
        .WithTags("Swarm")
        .Produces<BaseResponse<RollbackServiceResponse>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status500InternalServerError)
        .WithSummary("Rollback un service Docker Swarm")
        .WithDescription("Revient a la version precedente d'un service Docker Swarm")
        .RequireAuthorization();
    }
}
