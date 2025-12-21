using BackendAdmin.Application.Features.Swarm.DTOs;
using BackendAdmin.Application.Services;

namespace BackendAdmin.Application.Features.Swarm.Queries.GetContainerTop;

public class GetContainerTopHandler(IDockerSwarmService dockerSwarmService)
    : IQueryHandler<GetContainerTopQuery, GetContainerTopResult>
{
    public async Task<GetContainerTopResult> Handle(GetContainerTopQuery request, CancellationToken cancellationToken)
    {
        var top = await dockerSwarmService.GetContainerTopAsync(request.ContainerId, cancellationToken);
        return new GetContainerTopResult(top);
    }
}
