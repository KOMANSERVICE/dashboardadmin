using BackendAdmin.Application.Features.Swarm.DTOs;
using BackendAdmin.Application.Services;

namespace BackendAdmin.Application.Features.Swarm.Commands.DeployStack;

public class DeployStackHandler(IDockerSwarmService dockerSwarmService)
    : ICommandHandler<DeployStackCommand, DeployStackResult>
{
    public async Task<DeployStackResult> Handle(DeployStackCommand command, CancellationToken cancellationToken)
    {
        var request = new DeployStackRequest(
            Name: command.Name,
            ComposeFileContent: command.ComposeFileContent,
            Prune: command.Prune
        );

        var response = await dockerSwarmService.DeployStackAsync(request, cancellationToken);

        return new DeployStackResult(
            StackName: response.StackName,
            ServicesDeployed: response.ServicesDeployed,
            DeployedAt: response.DeployedAt
        );
    }
}
