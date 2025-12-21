using BackendAdmin.Application.Features.Swarm.Commands.DeleteVolume;

namespace BackendAdmin.Api.Endpoints.Swarm;

public class DeleteVolume : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("/api/swarm/volumes/{name}", async (string name, bool? force, ISender sender) =>
        {
            var command = new DeleteVolumeCommand(name, force ?? false);
            await sender.Send(command);

            return Results.NoContent();
        })
        .WithName("DeleteVolume")
        .WithTags("Swarm", "Volumes")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status409Conflict)
        .ProducesProblem(StatusCodes.Status500InternalServerError)
        .WithSummary("Supprime un volume Docker")
        .WithDescription("Supprime definitivement un volume Docker. Utilisez force=true pour supprimer un volume utilise par un container.")
        .RequireAuthorization();
    }
}
