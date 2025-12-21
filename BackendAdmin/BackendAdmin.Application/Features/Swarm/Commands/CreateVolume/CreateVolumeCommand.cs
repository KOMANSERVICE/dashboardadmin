namespace BackendAdmin.Application.Features.Swarm.Commands.CreateVolume;

public record CreateVolumeCommand(
    string Name,
    string Driver = "local",
    Dictionary<string, string>? Labels = null,
    Dictionary<string, string>? DriverOpts = null
) : ICommand<CreateVolumeResult>;

public record CreateVolumeResult(string VolumeName);
