using BackendAdmin.Application.Features.Swarm.Commands.DisconnectContainer;

namespace BackendAdmin.Api.Endpoints.Swarm;

public record DisconnectContainerApiRequest(
    string ContainerId,
    bool Force = false
);

public record DisconnectContainerApiResponse(
    bool Success,
    string NetworkName,
    string ContainerId
);

public class DisconnectContainerFromNetwork : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/swarm/networks/{name}/disconnect", async (string name, DisconnectContainerApiRequest request, ISender sender) =>
        {
            var command = new DisconnectContainerCommand(
                NetworkName: name,
                ContainerId: request.ContainerId,
                Force: request.Force
            );

            var result = await sender.Send(command);
            var response = result.Adapt<DisconnectContainerApiResponse>();
            var baseResponse = ResponseFactory.Success(
                response,
                $"Conteneur deconnecte du reseau '{name}' avec succes",
                StatusCodes.Status200OK);

            return Results.Ok(baseResponse);
        })
        .WithName("DisconnectContainerFromNetwork")
        .WithTags("Swarm", "Networks")
        .Produces<BaseResponse<DisconnectContainerApiResponse>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status500InternalServerError)
        .WithSummary("Deconnecte un conteneur d'un reseau")
        .WithDescription("Deconnecte un conteneur Docker d'un reseau specifique. Utilisez force=true pour forcer la deconnexion.")
        .RequireAuthorization();
    }
}
