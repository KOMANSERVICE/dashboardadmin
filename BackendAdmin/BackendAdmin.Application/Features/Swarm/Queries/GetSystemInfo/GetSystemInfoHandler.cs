using BackendAdmin.Application.Services;

namespace BackendAdmin.Application.Features.Swarm.Queries.GetSystemInfo;

public class GetSystemInfoHandler(IDockerSwarmService dockerSwarmService)
    : IQueryHandler<GetSystemInfoQuery, GetSystemInfoResult>
{
    public async Task<GetSystemInfoResult> Handle(GetSystemInfoQuery request, CancellationToken cancellationToken)
    {
        var systemInfo = await dockerSwarmService.GetSystemInfoAsync(cancellationToken);
        return new GetSystemInfoResult(systemInfo);
    }
}
