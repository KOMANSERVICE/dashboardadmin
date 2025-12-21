using BackendAdmin.Application.Features.Swarm.Commands.DeleteNetwork;

namespace BackendAdmin.Api.Endpoints.Swarm;

public class DeleteNetwork : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("/api/swarm/networks/{name}", async (string name, ISender sender) =>
        {
            var command = new DeleteNetworkCommand(name);
            await sender.Send(command);

            return Results.NoContent();
        })
        .WithName("DeleteNetwork")
        .WithTags("Swarm", "Networks")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status500InternalServerError)
        .WithSummary("Supprime un reseau Docker")
        .WithDescription("Supprime definitivement un reseau Docker. Les reseaux systeme (bridge, host, none, ingress, docker_gwbridge) ne peuvent pas etre supprimes.")
        .RequireAuthorization();
    }
}
