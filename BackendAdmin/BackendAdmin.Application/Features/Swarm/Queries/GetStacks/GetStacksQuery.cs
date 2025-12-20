using BackendAdmin.Application.Features.Swarm.DTOs;

namespace BackendAdmin.Application.Features.Swarm.Queries.GetStacks;

public record GetStacksQuery() : IQuery<GetStacksResult>;

public record GetStacksResult(List<StackDTO> Stacks);
