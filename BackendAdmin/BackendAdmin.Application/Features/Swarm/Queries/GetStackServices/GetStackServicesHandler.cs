using BackendAdmin.Application.Features.Swarm.DTOs;
using BackendAdmin.Application.Services;

namespace BackendAdmin.Application.Features.Swarm.Queries.GetStackServices;

public class GetStackServicesHandler(IDockerSwarmService dockerSwarmService)
    : IQueryHandler<GetStackServicesQuery, GetStackServicesResult>
{
    public async Task<GetStackServicesResult> Handle(GetStackServicesQuery request, CancellationToken cancellationToken)
    {
        var services = await dockerSwarmService.GetStackServicesAsync(request.StackName, cancellationToken);

        return new GetStackServicesResult(services.ToList());
    }
}
