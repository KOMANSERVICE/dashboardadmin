using BackendAdmin.Application.Features.Swarm.DTOs;

namespace BackendAdmin.Application.Features.Swarm.Queries.GetSwarmServices;

public record GetSwarmServicesQuery() : IQuery<GetSwarmServicesResult>;

public record GetSwarmServicesResult(List<SwarmServiceDTO> Services);
