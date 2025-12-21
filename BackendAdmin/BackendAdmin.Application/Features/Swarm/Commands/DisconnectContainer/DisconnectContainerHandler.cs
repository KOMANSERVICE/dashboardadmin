using BackendAdmin.Application.Features.Swarm.DTOs;
using BackendAdmin.Application.Services;

namespace BackendAdmin.Application.Features.Swarm.Commands.DisconnectContainer;

public class DisconnectContainerHandler(IDockerSwarmService dockerSwarmService)
    : ICommandHandler<DisconnectContainerCommand, DisconnectContainerResult>
{
    public async Task<DisconnectContainerResult> Handle(DisconnectContainerCommand command, CancellationToken cancellationToken)
    {
        var request = new DisconnectContainerRequest(
            ContainerId: command.ContainerId,
            Force: command.Force
        );

        await dockerSwarmService.DisconnectContainerFromNetworkAsync(command.NetworkName, request, cancellationToken);

        return new DisconnectContainerResult(true, command.NetworkName, command.ContainerId);
    }
}
