using BackendAdmin.Application.Features.Swarm.DTOs;

namespace BackendAdmin.Application.Features.Swarm.Queries.GetContainerChanges;

public record GetContainerChangesQuery(string ContainerId) : IQuery<GetContainerChangesResult>;

public record GetContainerChangesResult(ContainerChangesDTO Changes);
