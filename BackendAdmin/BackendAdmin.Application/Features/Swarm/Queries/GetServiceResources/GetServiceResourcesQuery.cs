using BackendAdmin.Application.Features.Swarm.DTOs;

namespace BackendAdmin.Application.Features.Swarm.Queries.GetServiceResources;

public record GetServiceResourcesQuery(string ServiceName) : IQuery<GetServiceResourcesResult>;

public record GetServiceResourcesResult(ServiceResourcesDTO Resources);
