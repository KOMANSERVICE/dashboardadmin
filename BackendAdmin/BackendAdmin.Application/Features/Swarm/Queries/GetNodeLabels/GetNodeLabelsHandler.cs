using BackendAdmin.Application.Services;

namespace BackendAdmin.Application.Features.Swarm.Queries.GetNodeLabels;

public class GetNodeLabelsHandler(IDockerSwarmService dockerSwarmService)
    : IQueryHandler<GetNodeLabelsQuery, GetNodeLabelsResult>
{
    public async Task<GetNodeLabelsResult> Handle(GetNodeLabelsQuery request, CancellationToken cancellationToken)
    {
        var labels = await dockerSwarmService.GetNodeLabelsAsync(request.NodeId, cancellationToken);
        return new GetNodeLabelsResult(request.NodeId, labels);
    }
}
