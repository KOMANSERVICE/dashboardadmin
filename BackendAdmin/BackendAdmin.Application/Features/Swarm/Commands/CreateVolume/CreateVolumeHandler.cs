using BackendAdmin.Application.Features.Swarm.DTOs;
using BackendAdmin.Application.Services;

namespace BackendAdmin.Application.Features.Swarm.Commands.CreateVolume;

public class CreateVolumeHandler(IDockerSwarmService dockerSwarmService)
    : ICommandHandler<CreateVolumeCommand, CreateVolumeResult>
{
    public async Task<CreateVolumeResult> Handle(CreateVolumeCommand command, CancellationToken cancellationToken)
    {
        var request = new CreateVolumeRequest(
            Name: command.Name,
            Driver: command.Driver,
            Labels: command.Labels,
            DriverOpts: command.DriverOpts
        );

        var volumeName = await dockerSwarmService.CreateVolumeAsync(request, cancellationToken);

        return new CreateVolumeResult(volumeName);
    }
}
