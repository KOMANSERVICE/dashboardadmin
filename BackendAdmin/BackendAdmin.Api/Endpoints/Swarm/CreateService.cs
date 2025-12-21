using BackendAdmin.Application.Features.Swarm.DTOs;
using BackendAdmin.Application.Features.Swarm.Commands.CreateService;

namespace BackendAdmin.Api.Endpoints.Swarm;

public record CreateServiceResponse(string ServiceId, string ServiceName);

public class CreateService : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/swarm/services", async (CreateServiceRequest request, ISender sender) =>
        {
            var command = new CreateServiceCommand(
                Name: request.Name,
                Image: request.Image,
                Replicas: request.Replicas,
                Ports: request.Ports,
                Env: request.Env,
                Labels: request.Labels
            );
            var result = await sender.Send(command);
            var response = result.Adapt<CreateServiceResponse>();
            var baseResponse = ResponseFactory.Success(
                response,
                $"Service '{request.Name}' cree avec succes",
                StatusCodes.Status201Created);

            return Results.Created($"/api/swarm/services/{request.Name}", baseResponse);
        })
        .WithName("CreateSwarmService")
        .WithTags("Swarm")
        .Produces<BaseResponse<CreateServiceResponse>>(StatusCodes.Status201Created)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status409Conflict)
        .ProducesProblem(StatusCodes.Status500InternalServerError)
        .WithSummary("Cree un nouveau service Docker Swarm")
        .WithDescription("Deploie un nouveau service Docker Swarm avec les parametres specifies (name, image, replicas, ports, env)")
        .RequireAuthorization();
    }
}
