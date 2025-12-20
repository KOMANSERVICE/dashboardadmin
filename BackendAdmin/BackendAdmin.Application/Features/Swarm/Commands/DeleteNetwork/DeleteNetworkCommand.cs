namespace BackendAdmin.Application.Features.Swarm.Commands.DeleteNetwork;

public record DeleteNetworkCommand(string NetworkName) : ICommand<DeleteNetworkResult>;

public record DeleteNetworkResult(bool Success);
