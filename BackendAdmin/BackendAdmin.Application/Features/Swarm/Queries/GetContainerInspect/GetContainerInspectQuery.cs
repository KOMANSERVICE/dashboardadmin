using BackendAdmin.Application.Features.Swarm.DTOs;

namespace BackendAdmin.Application.Features.Swarm.Queries.GetContainerInspect;

public record GetContainerInspectQuery(string ContainerId) : IQuery<GetContainerInspectResult>;

public record GetContainerInspectResult(ContainerDetailsDTO Details);
