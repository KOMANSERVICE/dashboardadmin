using BackendAdmin.Application.Features.Swarm.DTOs;
using BackendAdmin.Application.Services;

namespace BackendAdmin.Application.Features.Swarm.Queries.GetContainerStats;

public class GetContainerStatsHandler(IDockerSwarmService dockerSwarmService)
    : IQueryHandler<GetContainerStatsQuery, GetContainerStatsResult>
{
    public async Task<GetContainerStatsResult> Handle(GetContainerStatsQuery request, CancellationToken cancellationToken)
    {
        var stats = await dockerSwarmService.GetContainerStatsAsync(request.ContainerId, cancellationToken);
        return new GetContainerStatsResult(stats);
    }
}
