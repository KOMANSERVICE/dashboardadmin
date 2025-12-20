namespace BackendAdmin.Application.Features.Swarm.Commands.ConnectContainer;

public record ConnectContainerCommand(
    string NetworkName,
    string ContainerId,
    string? IpAddress = null
) : ICommand<ConnectContainerResult>;

public record ConnectContainerResult(bool Success, string NetworkName, string ContainerId);
