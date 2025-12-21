using BackendAdmin.Application.Features.Swarm.DTOs;
using BackendAdmin.Application.Services;

namespace BackendAdmin.Application.Features.Swarm.Queries.GetServiceLogs;

public class GetServiceLogsHandler(IDockerSwarmService dockerSwarmService)
    : IQueryHandler<GetServiceLogsQuery, GetServiceLogsResult>
{
    public async Task<GetServiceLogsResult> Handle(GetServiceLogsQuery request, CancellationToken cancellationToken)
    {
        var logs = await dockerSwarmService.GetServiceLogsAsync(
            request.ServiceName,
            request.Tail,
            request.Since,
            cancellationToken);

        var dto = new ServiceLogsDTO(
            ServiceName: request.ServiceName,
            Logs: logs,
            FetchedAt: DateTime.UtcNow
        );

        return new GetServiceLogsResult(dto);
    }
}
