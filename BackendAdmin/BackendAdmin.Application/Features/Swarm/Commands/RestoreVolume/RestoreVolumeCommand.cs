using BackendAdmin.Application.Features.Swarm.DTOs;

namespace BackendAdmin.Application.Features.Swarm.Commands.RestoreVolume;

public record RestoreVolumeCommand(
    string VolumeName,
    string SourcePath
) : ICommand<RestoreVolumeResult>;

public record RestoreVolumeResult(
    string VolumeName,
    string SourcePath,
    DateTime RestoreDate
);
