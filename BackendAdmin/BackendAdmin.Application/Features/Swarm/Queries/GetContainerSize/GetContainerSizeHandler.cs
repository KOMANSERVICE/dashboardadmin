using BackendAdmin.Application.Features.Swarm.DTOs;
using BackendAdmin.Application.Services;

namespace BackendAdmin.Application.Features.Swarm.Queries.GetContainerSize;

public class GetContainerSizeHandler(IDockerSwarmService dockerSwarmService)
    : IQueryHandler<GetContainerSizeQuery, GetContainerSizeResult>
{
    public async Task<GetContainerSizeResult> Handle(GetContainerSizeQuery request, CancellationToken cancellationToken)
    {
        var size = await dockerSwarmService.GetContainerSizeAsync(request.ContainerId, cancellationToken);
        return new GetContainerSizeResult(size);
    }
}
