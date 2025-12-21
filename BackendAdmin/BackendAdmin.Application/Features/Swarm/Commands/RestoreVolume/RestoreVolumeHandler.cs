using BackendAdmin.Application.Services;

namespace BackendAdmin.Application.Features.Swarm.Commands.RestoreVolume;

public class RestoreVolumeHandler(IDockerSwarmService dockerSwarmService)
    : ICommandHandler<RestoreVolumeCommand, RestoreVolumeResult>
{
    public async Task<RestoreVolumeResult> Handle(RestoreVolumeCommand command, CancellationToken cancellationToken)
    {
        var response = await dockerSwarmService.RestoreVolumeAsync(
            command.VolumeName,
            command.SourcePath,
            cancellationToken);

        return new RestoreVolumeResult(
            VolumeName: response.VolumeName,
            SourcePath: response.SourcePath,
            RestoreDate: response.RestoreDate
        );
    }
}
