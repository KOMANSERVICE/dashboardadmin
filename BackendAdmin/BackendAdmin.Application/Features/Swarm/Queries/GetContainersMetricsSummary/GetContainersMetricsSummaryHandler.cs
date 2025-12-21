using BackendAdmin.Application.Features.Swarm.DTOs;
using BackendAdmin.Application.Services;

namespace BackendAdmin.Application.Features.Swarm.Queries.GetContainersMetricsSummary;

public class GetContainersMetricsSummaryHandler(IDockerSwarmService dockerSwarmService)
    : IQueryHandler<GetContainersMetricsSummaryQuery, GetContainersMetricsSummaryResult>
{
    public async Task<GetContainersMetricsSummaryResult> Handle(GetContainersMetricsSummaryQuery request, CancellationToken cancellationToken)
    {
        var containers = await dockerSwarmService.GetAllContainersMetricsSummaryAsync(cancellationToken);
        return new GetContainersMetricsSummaryResult(containers);
    }
}
