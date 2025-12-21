using BackendAdmin.Application.Features.Swarm.DTOs;

namespace BackendAdmin.Application.Features.Swarm.Queries.GetContainerTop;

public record GetContainerTopQuery(string ContainerId) : IQuery<GetContainerTopResult>;

public record GetContainerTopResult(ContainerTopDTO Top);
