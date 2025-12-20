using BackendAdmin.Application.Features.Swarm.DTOs;
using BackendAdmin.Application.Services;

namespace BackendAdmin.Application.Features.Swarm.Commands.ConnectContainer;

public class ConnectContainerHandler(IDockerSwarmService dockerSwarmService)
    : ICommandHandler<ConnectContainerCommand, ConnectContainerResult>
{
    public async Task<ConnectContainerResult> Handle(ConnectContainerCommand command, CancellationToken cancellationToken)
    {
        var request = new ConnectContainerRequest(
            ContainerId: command.ContainerId,
            IpAddress: command.IpAddress
        );

        await dockerSwarmService.ConnectContainerToNetworkAsync(command.NetworkName, request, cancellationToken);

        return new ConnectContainerResult(true, command.NetworkName, command.ContainerId);
    }
}
