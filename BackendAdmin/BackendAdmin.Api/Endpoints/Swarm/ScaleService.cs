using BackendAdmin.Application.Features.Swarm.Commands.ScaleService;
using BackendAdmin.Application.Features.Swarm.DTOs;

namespace BackendAdmin.Api.Endpoints.Swarm;

public class ScaleService : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/swarm/services/{name}/scale", async (string name, ScaleServiceRequest request, ISender sender) =>
        {
            var command = new ScaleServiceCommand(name, request.Replicas);
            var result = await sender.Send(command);
            var response = result.Response;
            var baseResponse = ResponseFactory.Success(
                response,
                "Service scale avec succes",
                StatusCodes.Status200OK);

            return Results.Ok(baseResponse);
        })
        .WithName("ScaleService")
        .WithTags("Swarm")
        .Produces<BaseResponse<ScaleServiceResponse>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status500InternalServerError)
        .WithSummary("Scale un service")
        .WithDescription("Modifie le nombre de replicas d'un service Docker Swarm")
        .RequireAuthorization();
    }
}
