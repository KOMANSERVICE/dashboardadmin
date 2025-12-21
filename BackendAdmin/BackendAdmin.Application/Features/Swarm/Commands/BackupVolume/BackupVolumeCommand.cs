using BackendAdmin.Application.Features.Swarm.DTOs;

namespace BackendAdmin.Application.Features.Swarm.Commands.BackupVolume;

public record BackupVolumeCommand(
    string VolumeName,
    string DestinationPath
) : ICommand<BackupVolumeResult>;

public record BackupVolumeResult(
    string VolumeName,
    string BackupPath,
    DateTime BackupDate,
    long SizeBytes
);
