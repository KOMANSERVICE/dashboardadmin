using BackendAdmin.Application.Features.Swarm.DTOs;
using BackendAdmin.Application.Services;

namespace BackendAdmin.Application.Features.Swarm.Queries.GetDockerEvents;

public class GetDockerEventsHandler(IDockerSwarmService dockerSwarmService)
    : IQueryHandler<GetDockerEventsQuery, GetDockerEventsResult>
{
    public async Task<GetDockerEventsResult> Handle(GetDockerEventsQuery request, CancellationToken cancellationToken)
    {
        var events = await dockerSwarmService.GetDockerEventsAsync(request.Since, request.Until, cancellationToken);
        return new GetDockerEventsResult(events);
    }
}
