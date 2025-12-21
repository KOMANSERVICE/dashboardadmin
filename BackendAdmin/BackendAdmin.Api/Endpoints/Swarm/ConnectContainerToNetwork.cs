using BackendAdmin.Application.Features.Swarm.Commands.ConnectContainer;

namespace BackendAdmin.Api.Endpoints.Swarm;

public record ConnectContainerApiRequest(
    string ContainerId,
    string? IpAddress = null
);

public record ConnectContainerApiResponse(
    bool Success,
    string NetworkName,
    string ContainerId
);

public class ConnectContainerToNetwork : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/swarm/networks/{name}/connect", async (string name, ConnectContainerApiRequest request, ISender sender) =>
        {
            var command = new ConnectContainerCommand(
                NetworkName: name,
                ContainerId: request.ContainerId,
                IpAddress: request.IpAddress
            );

            var result = await sender.Send(command);
            var response = result.Adapt<ConnectContainerApiResponse>();
            var baseResponse = ResponseFactory.Success(
                response,
                $"Conteneur connecte au reseau '{name}' avec succes",
                StatusCodes.Status200OK);

            return Results.Ok(baseResponse);
        })
        .WithName("ConnectContainerToNetwork")
        .WithTags("Swarm", "Networks")
        .Produces<BaseResponse<ConnectContainerApiResponse>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status500InternalServerError)
        .WithSummary("Connecte un conteneur a un reseau")
        .WithDescription("Connecte un conteneur Docker a un reseau specifique. Une adresse IP peut etre specifiee optionnellement.")
        .RequireAuthorization();
    }
}
