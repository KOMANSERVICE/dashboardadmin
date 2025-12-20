namespace BackendAdmin.Application.Features.Swarm.Commands.DeleteVolume;

public record DeleteVolumeCommand(
    string Name,
    bool Force = false
) : ICommand<DeleteVolumeResult>;

public record DeleteVolumeResult(bool Success);
