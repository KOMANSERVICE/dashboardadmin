using BackendAdmin.Application.Features.Swarm.Commands.PushImage;

namespace BackendAdmin.Api.Endpoints.Swarm;

public record PushImageApiRequest(
    string? Tag = null,
    string? Registry = null
);

public record PushImageApiResponse(
    string ImageName,
    string Tag,
    string Status,
    DateTime PushedAt
);

public class PushImage : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/swarm/images/{id}/push", async (string id, PushImageApiRequest request, ISender sender) =>
        {
            var command = new PushImageCommand(
                ImageId: id,
                Tag: request.Tag,
                Registry: request.Registry
            );
            var result = await sender.Send(command);
            var response = result.Adapt<PushImageApiResponse>();
            var baseResponse = ResponseFactory.Success(
                response,
                $"Image {result.ImageName}:{result.Tag} envoyee avec succes",
                StatusCodes.Status200OK);

            return Results.Ok(baseResponse);
        })
        .WithName("PushImage")
        .WithTags("Swarm", "Images")
        .Produces<BaseResponse<PushImageApiResponse>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status500InternalServerError)
        .WithSummary("Push une image Docker")
        .WithDescription("Envoie une image Docker vers un registry (Docker Hub par defaut)")
        .RequireAuthorization();
    }
}
