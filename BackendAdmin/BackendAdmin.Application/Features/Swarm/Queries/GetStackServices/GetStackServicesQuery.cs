using BackendAdmin.Application.Features.Swarm.DTOs;

namespace BackendAdmin.Application.Features.Swarm.Queries.GetStackServices;

public record GetStackServicesQuery(string StackName) : IQuery<GetStackServicesResult>;

public record GetStackServicesResult(List<StackServiceDTO> Services);
