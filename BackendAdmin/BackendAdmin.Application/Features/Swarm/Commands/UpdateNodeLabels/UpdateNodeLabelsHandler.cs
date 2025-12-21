using BackendAdmin.Application.Services;

namespace BackendAdmin.Application.Features.Swarm.Commands.UpdateNodeLabels;

public class UpdateNodeLabelsHandler(IDockerSwarmService dockerSwarmService)
    : ICommandHandler<UpdateNodeLabelsCommand, UpdateNodeLabelsResult>
{
    public async Task<UpdateNodeLabelsResult> Handle(UpdateNodeLabelsCommand request, CancellationToken cancellationToken)
    {
        await dockerSwarmService.UpdateNodeLabelsAsync(request.NodeId, request.Labels, cancellationToken);
        return new UpdateNodeLabelsResult("Labels du noeud mis a jour avec succes");
    }
}
