using BackendAdmin.Application.Services;

namespace BackendAdmin.Application.Features.Swarm.Commands.PruneAll;

public class PruneAllHandler(IDockerSwarmService dockerSwarmService)
    : ICommandHandler<PruneAllCommand, PruneAllResult>
{
    public async Task<PruneAllResult> Handle(PruneAllCommand command, CancellationToken cancellationToken)
    {
        var response = await dockerSwarmService.PruneAllAsync(cancellationToken);
        return new PruneAllResult(response);
    }
}
