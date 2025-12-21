using BackendAdmin.Application.Features.Swarm.DTOs;
using BackendAdmin.Application.Services;

namespace BackendAdmin.Application.Features.Swarm.Queries.GetNodeDetails;

public class GetNodeDetailsHandler(IDockerSwarmService dockerSwarmService)
    : IQueryHandler<GetNodeDetailsQuery, GetNodeDetailsResult>
{
    public async Task<GetNodeDetailsResult> Handle(GetNodeDetailsQuery request, CancellationToken cancellationToken)
    {
        var node = await dockerSwarmService.GetNodeByIdAsync(request.NodeId, cancellationToken);

        if (node == null)
        {
            throw new NotFoundException($"Noeud '{request.NodeId}' non trouve");
        }

        var nodeDto = new NodeDetailsDTO(
            Id: node.ID,
            Hostname: node.Description?.Hostname ?? "unknown",
            Role: node.Spec?.Role ?? "unknown",
            State: node.Status?.State?.ToString() ?? "unknown",
            Availability: node.Spec?.Availability ?? "unknown",
            EngineVersion: node.Description?.Engine?.EngineVersion ?? "unknown",
            NanoCPUs: node.Description?.Resources?.NanoCPUs ?? 0,
            MemoryBytes: node.Description?.Resources?.MemoryBytes ?? 0,
            CreatedAt: node.CreatedAt,
            UpdatedAt: node.UpdatedAt,
            StatusMessage: node.Status?.Message,
            StatusAddr: node.Status?.Addr,
            Platform: node.Description?.Platform?.OS,
            Architecture: node.Description?.Platform?.Architecture,
            Labels: node.Spec?.Labels != null
                ? new Dictionary<string, string>(node.Spec.Labels)
                : new Dictionary<string, string>()
        );

        return new GetNodeDetailsResult(nodeDto);
    }
}
