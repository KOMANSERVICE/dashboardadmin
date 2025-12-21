using BackendAdmin.Application.Services;

namespace BackendAdmin.Application.Features.Swarm.Commands.DeleteNetwork;

public class DeleteNetworkHandler(IDockerSwarmService dockerSwarmService)
    : ICommandHandler<DeleteNetworkCommand, DeleteNetworkResult>
{
    private static readonly HashSet<string> ProtectedNetworks = new(StringComparer.OrdinalIgnoreCase)
    {
        "bridge",
        "host",
        "none",
        "ingress",
        "docker_gwbridge"
    };

    public async Task<DeleteNetworkResult> Handle(DeleteNetworkCommand command, CancellationToken cancellationToken)
    {
        // Check if this is a protected system network
        if (ProtectedNetworks.Contains(command.NetworkName))
        {
            throw new BadRequestException($"Le reseau '{command.NetworkName}' est un reseau systeme et ne peut pas etre supprime");
        }

        await dockerSwarmService.DeleteNetworkAsync(command.NetworkName, cancellationToken);

        return new DeleteNetworkResult(true);
    }
}
