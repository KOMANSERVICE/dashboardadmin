using BackendAdmin.Application.Features.Swarm.DTOs;

namespace BackendAdmin.Application.Features.Swarm.Queries.GetNodes;

public record GetNodesQuery() : IQuery<GetNodesResult>;

public record GetNodesResult(List<NodeDTO> Nodes);
