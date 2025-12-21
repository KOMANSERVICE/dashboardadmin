using BackendAdmin.Application.Features.Swarm.DTOs;
using BackendAdmin.Application.Services;

namespace BackendAdmin.Application.Features.Swarm.Queries.GetServiceTasks;

public class GetServiceTasksHandler(IDockerSwarmService dockerSwarmService)
    : IQueryHandler<GetServiceTasksQuery, GetServiceTasksResult>
{
    public async Task<GetServiceTasksResult> Handle(GetServiceTasksQuery request, CancellationToken cancellationToken)
    {
        var tasks = await dockerSwarmService.GetServiceTasksAsync(request.ServiceName, cancellationToken);

        var taskDtos = tasks.Select(t => new ServiceTaskDTO(
            Id: t.ID,
            NodeId: t.NodeID ?? "unassigned",
            State: t.Status?.State.ToString() ?? "unknown",
            ContainerId: t.Status?.ContainerStatus?.ContainerID,
            Message: t.Status?.Message,
            CreatedAt: t.CreatedAt,
            UpdatedAt: t.UpdatedAt
        )).ToList();

        return new GetServiceTasksResult(taskDtos);
    }
}
