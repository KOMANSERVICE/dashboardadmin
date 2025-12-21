using BackendAdmin.Application.Services;

namespace BackendAdmin.Application.Features.Swarm.Commands.DeleteVolume;

public class DeleteVolumeHandler(IDockerSwarmService dockerSwarmService)
    : ICommandHandler<DeleteVolumeCommand, DeleteVolumeResult>
{
    public async Task<DeleteVolumeResult> Handle(DeleteVolumeCommand command, CancellationToken cancellationToken)
    {
        await dockerSwarmService.DeleteVolumeAsync(command.Name, command.Force, cancellationToken);

        return new DeleteVolumeResult(true);
    }
}
