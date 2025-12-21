using BackendAdmin.Application.Features.Swarm.DTOs;

namespace BackendAdmin.Application.Features.Swarm.Queries.GetDockerEvents;

public record GetDockerEventsQuery(DateTime? Since = null, DateTime? Until = null) : IQuery<GetDockerEventsResult>;

public record GetDockerEventsResult(IList<DockerEventDTO> Events);
