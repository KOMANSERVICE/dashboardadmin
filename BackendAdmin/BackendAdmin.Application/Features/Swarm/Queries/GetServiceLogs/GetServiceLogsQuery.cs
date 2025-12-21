using BackendAdmin.Application.Features.Swarm.DTOs;

namespace BackendAdmin.Application.Features.Swarm.Queries.GetServiceLogs;

public record GetServiceLogsQuery(string ServiceName, int? Tail = null, string? Since = null)
    : IQuery<GetServiceLogsResult>;

public record GetServiceLogsResult(ServiceLogsDTO Logs);
