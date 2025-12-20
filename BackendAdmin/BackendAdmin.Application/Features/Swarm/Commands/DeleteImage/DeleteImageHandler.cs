using BackendAdmin.Application.Services;

namespace BackendAdmin.Application.Features.Swarm.Commands.DeleteImage;

public class DeleteImageHandler(IDockerSwarmService dockerSwarmService)
    : ICommandHandler<DeleteImageCommand, DeleteImageResult>
{
    public async Task<DeleteImageResult> Handle(DeleteImageCommand command, CancellationToken cancellationToken)
    {
        await dockerSwarmService.DeleteImageAsync(
            command.ImageId,
            command.Force,
            command.PruneChildren,
            cancellationToken);

        return new DeleteImageResult(true);
    }
}
