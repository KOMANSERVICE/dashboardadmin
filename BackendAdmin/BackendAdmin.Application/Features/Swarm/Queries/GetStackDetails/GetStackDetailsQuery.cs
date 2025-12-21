using BackendAdmin.Application.Features.Swarm.DTOs;

namespace BackendAdmin.Application.Features.Swarm.Queries.GetStackDetails;

public record GetStackDetailsQuery(string StackName) : IQuery<GetStackDetailsResult>;

public record GetStackDetailsResult(StackDetailsDTO? Stack);
