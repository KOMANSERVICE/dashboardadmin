using BackendAdmin.Application.Features.Swarm.DTOs;
using BackendAdmin.Application.Features.Swarm.Queries.GetDiskUsage;

namespace BackendAdmin.Api.Endpoints.Swarm;

public record GetDiskUsageResponse(DiskUsageDTO DiskUsage);

public class GetDiskUsage : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/swarm/system/disk", async (ISender sender) =>
        {
            var query = new GetDiskUsageQuery();
            var result = await sender.Send(query);
            var response = result.Adapt<GetDiskUsageResponse>();
            var baseResponse = ResponseFactory.Success(
                response,
                "Utilisation disque Docker recuperee avec succes",
                StatusCodes.Status200OK);

            return Results.Ok(baseResponse);
        })
        .WithName("GetDiskUsage")
        .WithTags("Swarm", "System")
        .Produces<BaseResponse<GetDiskUsageResponse>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status500InternalServerError)
        .WithSummary("Recupere l'utilisation disque Docker")
        .WithDescription("Retourne l'espace disque utilise par Docker: images, conteneurs, volumes, build cache avec un resume des espaces recuperables")
        .RequireAuthorization();
    }
}
