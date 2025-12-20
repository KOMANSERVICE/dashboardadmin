using BackendAdmin.Application.Features.Swarm.DTOs;
using BackendAdmin.Application.Features.Swarm.Commands.PullImage;

namespace BackendAdmin.Api.Endpoints.Swarm;

public record PullImageApiRequest(
    string Image,
    string? Tag = "latest",
    string? Registry = null
);

public record PullImageApiResponse(
    string ImageName,
    string Tag,
    string Status,
    DateTime PulledAt
);

public class PullImage : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/swarm/images/pull", async (PullImageApiRequest request, ISender sender) =>
        {
            var command = new PullImageCommand(
                Image: request.Image,
                Tag: request.Tag,
                Registry: request.Registry
            );
            var result = await sender.Send(command);
            var response = result.Adapt<PullImageApiResponse>();
            var baseResponse = ResponseFactory.Success(
                response,
                $"Image {request.Image}:{request.Tag ?? "latest"} telechargee avec succes",
                StatusCodes.Status200OK);

            return Results.Ok(baseResponse);
        })
        .WithName("PullImage")
        .WithTags("Swarm", "Images")
        .Produces<BaseResponse<PullImageApiResponse>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status500InternalServerError)
        .WithSummary("Telecharge une image Docker")
        .WithDescription("Telecharge une image Docker depuis un registry (Docker Hub par defaut)")
        .RequireAuthorization();
    }
}
