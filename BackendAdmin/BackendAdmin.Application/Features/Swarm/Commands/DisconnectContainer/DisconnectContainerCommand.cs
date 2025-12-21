namespace BackendAdmin.Application.Features.Swarm.Commands.DisconnectContainer;

public record DisconnectContainerCommand(
    string NetworkName,
    string ContainerId,
    bool Force = false
) : ICommand<DisconnectContainerResult>;

public record DisconnectContainerResult(bool Success, string NetworkName, string ContainerId);
