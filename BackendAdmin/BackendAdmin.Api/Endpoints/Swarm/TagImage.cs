using BackendAdmin.Application.Features.Swarm.Commands.TagImage;

namespace BackendAdmin.Api.Endpoints.Swarm;

public record TagImageApiRequest(
    string NewRepository,
    string NewTag
);

public record TagImageApiResponse(
    string SourceImage,
    string NewRepository,
    string NewTag
);

public class TagImage : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapPost("/api/swarm/images/{id}/tag", async (string id, TagImageApiRequest request, ISender sender) =>
        {
            var command = new TagImageCommand(
                ImageId: id,
                NewRepository: request.NewRepository,
                NewTag: request.NewTag
            );
            var result = await sender.Send(command);
            var response = result.Adapt<TagImageApiResponse>();
            var baseResponse = ResponseFactory.Success(
                response,
                $"Image taguee avec succes: {request.NewRepository}:{request.NewTag}",
                StatusCodes.Status200OK);

            return Results.Ok(baseResponse);
        })
        .WithName("TagImage")
        .WithTags("Swarm", "Images")
        .Produces<BaseResponse<TagImageApiResponse>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status404NotFound)
        .ProducesProblem(StatusCodes.Status400BadRequest)
        .ProducesProblem(StatusCodes.Status500InternalServerError)
        .WithSummary("Tag une image Docker")
        .WithDescription("Cree un nouveau tag pour une image Docker existante")
        .RequireAuthorization();
    }
}
