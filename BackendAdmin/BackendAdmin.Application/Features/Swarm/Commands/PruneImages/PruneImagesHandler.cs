using BackendAdmin.Application.Services;

namespace BackendAdmin.Application.Features.Swarm.Commands.PruneImages;

public class PruneImagesHandler(IDockerSwarmService dockerSwarmService)
    : ICommandHandler<PruneImagesCommand, PruneImagesResult>
{
    public async Task<PruneImagesResult> Handle(PruneImagesCommand command, CancellationToken cancellationToken)
    {
        var (count, spaceReclaimed, deletedImages) = await dockerSwarmService.PruneImagesAsync(
            command.Dangling,
            cancellationToken);

        return new PruneImagesResult(
            DeletedCount: count,
            SpaceReclaimed: spaceReclaimed,
            DeletedImages: deletedImages
        );
    }
}
