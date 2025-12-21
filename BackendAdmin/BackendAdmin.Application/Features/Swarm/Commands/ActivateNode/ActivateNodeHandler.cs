using BackendAdmin.Application.Services;

namespace BackendAdmin.Application.Features.Swarm.Commands.ActivateNode;

public class ActivateNodeHandler(IDockerSwarmService dockerSwarmService)
    : ICommandHandler<ActivateNodeCommand, ActivateNodeResult>
{
    public async Task<ActivateNodeResult> Handle(ActivateNodeCommand request, CancellationToken cancellationToken)
    {
        await dockerSwarmService.ActivateNodeAsync(request.NodeId, cancellationToken);
        return new ActivateNodeResult("Noeud active avec succes");
    }
}
