using BackendAdmin.Application.Features.Swarm.DTOs;
using BackendAdmin.Application.Services;

namespace BackendAdmin.Application.Features.Swarm.Queries.GetNodes;

public class GetNodesHandler(IDockerSwarmService dockerSwarmService)
    : IQueryHandler<GetNodesQuery, GetNodesResult>
{
    public async Task<GetNodesResult> Handle(GetNodesQuery request, CancellationToken cancellationToken)
    {
        var nodes = await dockerSwarmService.GetNodesAsync(cancellationToken);

        var nodeDtos = nodes.Select(n => new NodeDTO(
            Id: n.ID,
            Hostname: n.Description?.Hostname ?? "unknown",
            Role: n.Spec?.Role ?? "unknown",
            State: n.Status?.State?.ToString() ?? "unknown",
            Availability: n.Spec?.Availability ?? "unknown",
            EngineVersion: n.Description?.Engine?.EngineVersion ?? "unknown",
            NanoCPUs: n.Description?.Resources?.NanoCPUs ?? 0,
            MemoryBytes: n.Description?.Resources?.MemoryBytes ?? 0,
            CreatedAt: n.CreatedAt,
            UpdatedAt: n.UpdatedAt
        )).ToList();

        return new GetNodesResult(nodeDtos);
    }
}
