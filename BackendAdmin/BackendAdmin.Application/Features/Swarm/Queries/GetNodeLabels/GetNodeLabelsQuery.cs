namespace BackendAdmin.Application.Features.Swarm.Queries.GetNodeLabels;

public record GetNodeLabelsQuery(string NodeId) : IQuery<GetNodeLabelsResult>;

public record GetNodeLabelsResult(string NodeId, Dictionary<string, string> Labels);
