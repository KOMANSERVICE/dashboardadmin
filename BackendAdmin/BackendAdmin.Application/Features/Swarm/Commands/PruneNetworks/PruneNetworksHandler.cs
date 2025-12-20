using BackendAdmin.Application.Services;

namespace BackendAdmin.Application.Features.Swarm.Commands.PruneNetworks;

public class PruneNetworksHandler(IDockerSwarmService dockerSwarmService)
    : ICommandHandler<PruneNetworksCommand, PruneNetworksResult>
{
    public async Task<PruneNetworksResult> Handle(PruneNetworksCommand command, CancellationToken cancellationToken)
    {
        var (count, deletedNetworks) = await dockerSwarmService.PruneNetworksAsync(cancellationToken);

        return new PruneNetworksResult(count, deletedNetworks);
    }
}
