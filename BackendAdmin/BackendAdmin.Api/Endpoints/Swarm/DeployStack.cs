using BackendAdmin.Application.Features.Swarm.DTOs;
using BackendAdmin.Application.Features.Swarm.Commands.DeployStack;

namespace BackendAdmin.Api.Endpoints.Swarm;

public record DeployStackApiRequest(
    string Name,
    string ComposeFileContent,
    bool Prune = false
);

public record DeployStackApiResponse(
    string StackName,
    int ServicesDeployed,
    DateTime DeployedAt
);

public class DeployStack : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/swarm/stacks", async (DeployStackApiRequest request, ISender sender) =>
        {
            var command = new DeployStackCommand(
                Name: request.Name,
                ComposeFileContent: request.ComposeFileContent,
                Prune: request.Prune
            );
            var result = await sender.Send(command);
            var response = result.Adapt<DeployStackApiResponse>();
            var baseResponse = ResponseFactory.Success(
                response,
                $"Stack '{request.Name}' deployee avec succes",
                StatusCodes.Status201Created);

            return Results.Created($"/api/swarm/stacks/{request.Name}", baseResponse);
        })
        .WithName("DeployStack")
        .WithTags("Swarm", "Stacks")
        .Produces<BaseResponse<DeployStackApiResponse>>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status409Conflict)
        .ProducesProblem(StatusCodes.Status500InternalServerError)
        .WithSummary("Deploie une nouvelle stack Docker Swarm")
        .WithDescription("Deploie une stack Docker Swarm a partir d'un fichier Docker Compose. Utiliser l'option prune pour supprimer les services qui ne sont plus dans le compose file.")
        .RequireAuthorization();
    }
}
