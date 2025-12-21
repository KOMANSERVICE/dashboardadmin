using BackendAdmin.Application.Features.Swarm.DTOs;

namespace BackendAdmin.Application.Features.Swarm.Queries.GetContainers;

public record GetContainersQuery(bool All = true) : IQuery<GetContainersResult>;

public record GetContainersResult(List<ContainerDTO> Containers);
