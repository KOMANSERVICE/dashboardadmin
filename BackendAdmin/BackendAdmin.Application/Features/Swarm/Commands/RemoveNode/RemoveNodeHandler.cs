using BackendAdmin.Application.Services;

namespace BackendAdmin.Application.Features.Swarm.Commands.RemoveNode;

public class RemoveNodeHandler(IDockerSwarmService dockerSwarmService)
    : ICommandHandler<RemoveNodeCommand, RemoveNodeResult>
{
    public async Task<RemoveNodeResult> Handle(RemoveNodeCommand request, CancellationToken cancellationToken)
    {
        await dockerSwarmService.RemoveNodeAsync(request.NodeId, request.Force, cancellationToken);
        return new RemoveNodeResult("Noeud supprime du cluster avec succes");
    }
}
