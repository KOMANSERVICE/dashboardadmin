using BackendAdmin.Application.Features.Swarm.DTOs;
using BackendAdmin.Application.Services;

namespace BackendAdmin.Application.Features.Swarm.Commands.CreateNetwork;

public class CreateNetworkHandler(IDockerSwarmService dockerSwarmService)
    : ICommandHandler<CreateNetworkCommand, CreateNetworkResult>
{
    public async Task<CreateNetworkResult> Handle(CreateNetworkCommand command, CancellationToken cancellationToken)
    {
        var request = new CreateNetworkRequest(
            Name: command.Name,
            Driver: command.Driver,
            IsAttachable: command.IsAttachable,
            Subnet: command.Subnet,
            Gateway: command.Gateway,
            IpRange: command.IpRange,
            Labels: command.Labels,
            Options: command.Options
        );

        var networkId = await dockerSwarmService.CreateNetworkAsync(request, cancellationToken);

        return new CreateNetworkResult(networkId, command.Name);
    }
}
