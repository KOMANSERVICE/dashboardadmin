using BackendAdmin.Application.Features.Swarm.DTOs;
using BackendAdmin.Application.Features.Swarm.Commands.ExecContainer;

namespace BackendAdmin.Api.Endpoints.Swarm.Containers;

public record ExecContainerRequest(
    string Command,
    string[]? Args = null,
    bool AttachStdout = true,
    bool AttachStderr = true,
    string? WorkingDir = null,
    Dictionary<string, string>? Env = null
);

public record ExecContainerEndpointResponse(ContainerExecResponse Response);

public class ExecContainer : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/swarm/containers/{id}/exec", async (string id, ExecContainerRequest request, ISender sender) =>
        {
            var command = new ExecContainerCommand(
                ContainerId: id,
                Command: request.Command,
                Args: request.Args,
                AttachStdout: request.AttachStdout,
                AttachStderr: request.AttachStderr,
                WorkingDir: request.WorkingDir,
                Env: request.Env
            );
            var result = await sender.Send(command);
            var response = result.Adapt<ExecContainerEndpointResponse>();
            var baseResponse = ResponseFactory.Success(
                response,
                "Commande executee avec succes",
                StatusCodes.Status200OK);

            return Results.Ok(baseResponse);
        })
        .WithName("ExecContainer")
        .WithTags("Swarm", "Containers")
        .Produces<BaseResponse<ExecContainerEndpointResponse>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status500InternalServerError)
        .WithSummary("Execute une commande dans un conteneur")
        .WithDescription("Execute une commande dans un conteneur Docker en cours d'execution et retourne la sortie stdout/stderr")
        .RequireAuthorization();
    }
}
