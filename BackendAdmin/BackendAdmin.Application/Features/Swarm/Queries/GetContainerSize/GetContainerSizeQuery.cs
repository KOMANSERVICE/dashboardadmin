using BackendAdmin.Application.Features.Swarm.DTOs;

namespace BackendAdmin.Application.Features.Swarm.Queries.GetContainerSize;

public record GetContainerSizeQuery(string ContainerId) : IQuery<GetContainerSizeResult>;

public record GetContainerSizeResult(ContainerSizeDTO Size);
