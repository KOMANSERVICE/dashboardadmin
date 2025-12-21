using BackendAdmin.Application.Features.Swarm.DTOs;

namespace BackendAdmin.Application.Features.Swarm.Queries.GetNodeDetails;

public record GetNodeDetailsQuery(string NodeId) : IQuery<GetNodeDetailsResult>;

public record GetNodeDetailsResult(NodeDetailsDTO Node);
