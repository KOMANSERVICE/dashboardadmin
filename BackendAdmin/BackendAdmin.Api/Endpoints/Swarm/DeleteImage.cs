using BackendAdmin.Application.Features.Swarm.Commands.DeleteImage;

namespace BackendAdmin.Api.Endpoints.Swarm;

public class DeleteImage : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapDelete("/api/swarm/images/{id}", async (string id, bool? force, bool? pruneChildren, ISender sender) =>
        {
            var command = new DeleteImageCommand(id, force ?? false, pruneChildren ?? false);
            await sender.Send(command);

            return Results.NoContent();
        })
        .WithName("DeleteImage")
        .WithTags("Swarm", "Images")
        .Produces(StatusCodes.Status204NoContent)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status409Conflict)
        .ProducesProblem(StatusCodes.Status500InternalServerError)
        .WithSummary("Supprime une image Docker")
        .WithDescription("Supprime definitivement une image Docker. Utilisez force=true pour supprimer une image utilisee par un container.")
        .RequireAuthorization();
    }
}
