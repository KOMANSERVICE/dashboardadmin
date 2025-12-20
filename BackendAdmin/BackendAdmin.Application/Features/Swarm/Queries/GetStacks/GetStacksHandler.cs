using BackendAdmin.Application.Features.Swarm.DTOs;
using BackendAdmin.Application.Services;

namespace BackendAdmin.Application.Features.Swarm.Queries.GetStacks;

public class GetStacksHandler(IDockerSwarmService dockerSwarmService)
    : IQueryHandler<GetStacksQuery, GetStacksResult>
{
    public async Task<GetStacksResult> Handle(GetStacksQuery request, CancellationToken cancellationToken)
    {
        var stacks = await dockerSwarmService.GetStacksAsync(cancellationToken);

        return new GetStacksResult(stacks.ToList());
    }
}
