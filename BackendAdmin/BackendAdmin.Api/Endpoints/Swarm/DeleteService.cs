using BackendAdmin.Application.Features.Swarm.Commands.DeleteService;

namespace BackendAdmin.Api.Endpoints.Swarm;

public class DeleteService : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("/api/swarm/services/{name}", async (string name, ISender sender) =>
        {
            var command = new DeleteServiceCommand(name);
            await sender.Send(command);

            return Results.NoContent();
        })
        .WithName("DeleteSwarmService")
        .WithTags("Swarm")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status500InternalServerError)
        .WithSummary("Supprime un service Docker Swarm")
        .WithDescription("Supprime definitivement un service Docker Swarm du cluster")
        .RequireAuthorization();
    }
}
