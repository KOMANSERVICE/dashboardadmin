using BackendAdmin.Application.Features.Swarm.DTOs;
using BackendAdmin.Application.Services;

namespace BackendAdmin.Application.Features.Swarm.Queries.GetContainerLogs;

public class GetContainerLogsHandler(IDockerSwarmService dockerSwarmService)
    : IQueryHandler<GetContainerLogsQuery, GetContainerLogsResult>
{
    public async Task<GetContainerLogsResult> Handle(GetContainerLogsQuery request, CancellationToken cancellationToken)
    {
        var container = await dockerSwarmService.GetContainerByIdAsync(request.ContainerId, cancellationToken);
        if (container == null)
        {
            throw new NotFoundException($"Conteneur '{request.ContainerId}' non trouve");
        }

        var logs = await dockerSwarmService.GetContainerLogsAsync(
            request.ContainerId,
            request.Tail,
            request.Timestamps,
            cancellationToken);

        var containerName = container.Name?.TrimStart('/') ?? request.ContainerId;

        return new GetContainerLogsResult(new ContainerLogsDTO(
            ContainerId: request.ContainerId,
            ContainerName: containerName,
            Logs: logs,
            FetchedAt: DateTime.UtcNow
        ));
    }
}
