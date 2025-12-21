using BackendAdmin.Application.Services;

namespace BackendAdmin.Application.Features.Swarm.Commands.BackupVolume;

public class BackupVolumeHandler(IDockerSwarmService dockerSwarmService)
    : ICommandHandler<BackupVolumeCommand, BackupVolumeResult>
{
    public async Task<BackupVolumeResult> Handle(BackupVolumeCommand command, CancellationToken cancellationToken)
    {
        var response = await dockerSwarmService.BackupVolumeAsync(
            command.VolumeName,
            command.DestinationPath,
            cancellationToken);

        return new BackupVolumeResult(
            VolumeName: response.VolumeName,
            BackupPath: response.BackupPath,
            BackupDate: response.BackupDate,
            SizeBytes: response.SizeBytes
        );
    }
}
