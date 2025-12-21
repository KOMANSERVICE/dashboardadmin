using BackendAdmin.Application.Services;

namespace BackendAdmin.Application.Features.Swarm.Commands.PruneVolumes;

public class PruneVolumesHandler(IDockerSwarmService dockerSwarmService)
    : ICommandHandler<PruneVolumesCommand, PruneVolumesResult>
{
    public async Task<PruneVolumesResult> Handle(PruneVolumesCommand command, CancellationToken cancellationToken)
    {
        var (count, spaceReclaimed, deletedVolumes) = await dockerSwarmService.PruneVolumesAsync(cancellationToken);

        return new PruneVolumesResult(count, spaceReclaimed, deletedVolumes);
    }
}
