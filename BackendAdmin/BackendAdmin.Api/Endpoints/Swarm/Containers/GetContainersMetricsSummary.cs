using BackendAdmin.Application.Features.Swarm.DTOs;
using BackendAdmin.Application.Features.Swarm.Queries.GetContainersMetricsSummary;

namespace BackendAdmin.Api.Endpoints.Swarm.Containers;

public record GetContainersMetricsSummaryResponse(IList<ContainerMetricsSummaryDTO> Containers);

public class GetContainersMetricsSummary : ICarterModule
{
    public void AddRoutes(IEndpointRouteBuilder app)
    {
        app.MapGet("/api/swarm/containers/metrics", async (ISender sender) =>
        {
            var query = new GetContainersMetricsSummaryQuery();
            var result = await sender.Send(query);
            var response = result.Adapt<GetContainersMetricsSummaryResponse>();
            var baseResponse = ResponseFactory.Success(
                response,
                "Resume des metriques des conteneurs recupere avec succes",
                StatusCodes.Status200OK);

            return Results.Ok(baseResponse);
        })
        .WithName("GetContainersMetricsSummary")
        .WithTags("Swarm", "Containers", "Metrics")
        .Produces<BaseResponse<GetContainersMetricsSummaryResponse>>(StatusCodes.Status200OK)
        .ProducesProblem(StatusCodes.Status500InternalServerError)
        .WithSummary("Recupere le resume des metriques de tous les conteneurs")
        .WithDescription("Retourne un resume des metriques (CPU, memoire, sante, uptime, redemarrages) de tous les conteneurs en cours d'execution")
        .RequireAuthorization();
    }
}
