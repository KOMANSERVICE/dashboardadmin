using BackendAdmin.Application.Services;

namespace BackendAdmin.Application.Features.Swarm.Commands.PromoteNode;

public class PromoteNodeHandler(IDockerSwarmService dockerSwarmService)
    : ICommandHandler<PromoteNodeCommand, PromoteNodeResult>
{
    public async Task<PromoteNodeResult> Handle(PromoteNodeCommand request, CancellationToken cancellationToken)
    {
        await dockerSwarmService.PromoteNodeAsync(request.NodeId, cancellationToken);
        return new PromoteNodeResult("Noeud promu en manager avec succes");
    }
}
