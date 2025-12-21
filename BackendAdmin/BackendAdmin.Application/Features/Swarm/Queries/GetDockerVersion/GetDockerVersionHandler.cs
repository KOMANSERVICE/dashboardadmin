using BackendAdmin.Application.Services;

namespace BackendAdmin.Application.Features.Swarm.Queries.GetDockerVersion;

public class GetDockerVersionHandler(IDockerSwarmService dockerSwarmService)
    : IQueryHandler<GetDockerVersionQuery, GetDockerVersionResult>
{
    public async Task<GetDockerVersionResult> Handle(GetDockerVersionQuery request, CancellationToken cancellationToken)
    {
        var version = await dockerSwarmService.GetDockerVersionAsync(cancellationToken);
        return new GetDockerVersionResult(version);
    }
}
