using BackendAdmin.Application.Services;

namespace BackendAdmin.Application.Features.Swarm.Commands.DemoteNode;

public class DemoteNodeHandler(IDockerSwarmService dockerSwarmService)
    : ICommandHandler<DemoteNodeCommand, DemoteNodeResult>
{
    public async Task<DemoteNodeResult> Handle(DemoteNodeCommand request, CancellationToken cancellationToken)
    {
        await dockerSwarmService.DemoteNodeAsync(request.NodeId, cancellationToken);
        return new DemoteNodeResult("Noeud retrograde en worker avec succes");
    }
}
