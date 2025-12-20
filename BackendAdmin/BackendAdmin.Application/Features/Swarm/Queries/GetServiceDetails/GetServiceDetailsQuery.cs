using BackendAdmin.Application.Features.Swarm.DTOs;

namespace BackendAdmin.Application.Features.Swarm.Queries.GetServiceDetails;

public record GetServiceDetailsQuery(string ServiceName) : IQuery<GetServiceDetailsResult>;

public record GetServiceDetailsResult(ServiceDetailsDTO Service);
