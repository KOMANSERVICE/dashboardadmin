using BackendAdmin.Application.Services;

namespace BackendAdmin.Application.Features.Swarm.Commands.DrainNode;

public class DrainNodeHandler(IDockerSwarmService dockerSwarmService)
    : ICommandHandler<DrainNodeCommand, DrainNodeResult>
{
    public async Task<DrainNodeResult> Handle(DrainNodeCommand request, CancellationToken cancellationToken)
    {
        await dockerSwarmService.DrainNodeAsync(request.NodeId, cancellationToken);
        return new DrainNodeResult("Noeud mis en mode drain avec succes");
    }
}
