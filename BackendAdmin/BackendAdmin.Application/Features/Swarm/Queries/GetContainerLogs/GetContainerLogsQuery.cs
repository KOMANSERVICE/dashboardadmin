using BackendAdmin.Application.Features.Swarm.DTOs;

namespace BackendAdmin.Application.Features.Swarm.Queries.GetContainerLogs;

public record GetContainerLogsQuery(string ContainerId, int? Tail = null, bool Timestamps = false) : IQuery<GetContainerLogsResult>;

public record GetContainerLogsResult(ContainerLogsDTO Logs);
