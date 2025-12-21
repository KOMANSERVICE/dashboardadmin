using BackendAdmin.Application.Services;

namespace BackendAdmin.Application.Features.Swarm.Queries.GetDiskUsage;

public class GetDiskUsageHandler(IDockerSwarmService dockerSwarmService)
    : IQueryHandler<GetDiskUsageQuery, GetDiskUsageResult>
{
    public async Task<GetDiskUsageResult> Handle(GetDiskUsageQuery request, CancellationToken cancellationToken)
    {
        var diskUsage = await dockerSwarmService.GetDiskUsageAsync(cancellationToken);
        return new GetDiskUsageResult(diskUsage);
    }
}
