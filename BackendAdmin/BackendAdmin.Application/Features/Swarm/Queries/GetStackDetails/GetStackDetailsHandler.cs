using BackendAdmin.Application.Features.Swarm.DTOs;
using BackendAdmin.Application.Services;

namespace BackendAdmin.Application.Features.Swarm.Queries.GetStackDetails;

public class GetStackDetailsHandler(IDockerSwarmService dockerSwarmService)
    : IQueryHandler<GetStackDetailsQuery, GetStackDetailsResult>
{
    public async Task<GetStackDetailsResult> Handle(GetStackDetailsQuery request, CancellationToken cancellationToken)
    {
        var stack = await dockerSwarmService.GetStackByNameAsync(request.StackName, cancellationToken);

        if (stack == null)
        {
            throw new NotFoundException($"Stack '{request.StackName}' non trouvee");
        }

        return new GetStackDetailsResult(stack);
    }
}
