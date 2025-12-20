using BackendAdmin.Application.Features.Swarm.DTOs;
using BackendAdmin.Application.Features.Swarm.Commands.UpdateService;

namespace BackendAdmin.Api.Endpoints.Swarm;

public record UpdateServiceResponse(string ServiceName);

public class UpdateService : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPut("/api/swarm/services/{name}", async (string name, UpdateServiceRequest request, ISender sender) =>
        {
            var command = new UpdateServiceCommand(
                ServiceName: name,
                Image: request.Image,
                Replicas: request.Replicas,
                Env: request.Env,
                Labels: request.Labels
            );
            var result = await sender.Send(command);
            var response = result.Adapt<UpdateServiceResponse>();
            var baseResponse = ResponseFactory.Success(
                response,
                $"Service '{name}' mis a jour avec succes",
                StatusCodes.Status200OK);

            return Results.Ok(baseResponse);
        })
        .WithName("UpdateSwarmService")
        .WithTags("Swarm")
        .Produces<BaseResponse<UpdateServiceResponse>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status500InternalServerError)
        .WithSummary("Met a jour un service Docker Swarm")
        .WithDescription("Met a jour un service Docker Swarm existant (image, replicas, env, labels)")
        .RequireAuthorization();
    }
}
