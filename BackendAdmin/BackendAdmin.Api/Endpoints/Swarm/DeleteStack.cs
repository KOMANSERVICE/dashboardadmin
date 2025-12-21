using BackendAdmin.Application.Features.Swarm.Commands.DeleteStack;

namespace BackendAdmin.Api.Endpoints.Swarm;

public class DeleteStack : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("/api/swarm/stacks/{name}", async (string name, ISender sender) =>
        {
            var command = new DeleteStackCommand(name);
            await sender.Send(command);

            return Results.NoContent();
        })
        .WithName("DeleteStack")
        .WithTags("Swarm", "Stacks")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status500InternalServerError)
        .WithSummary("Supprime une stack Docker Swarm")
        .WithDescription("Supprime definitivement une stack Docker Swarm et tous ses services. Cette operation est irreversible.")
        .RequireAuthorization();
    }
}
