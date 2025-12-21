using BackendAdmin.Application.Features.Swarm.DTOs;

namespace BackendAdmin.Application.Features.Swarm.Commands.DeployStack;

public record DeployStackCommand(
    string Name,
    string ComposeFileContent,
    bool Prune = false
) : ICommand<DeployStackResult>;

public record DeployStackResult(
    string StackName,
    int ServicesDeployed,
    DateTime DeployedAt
);
