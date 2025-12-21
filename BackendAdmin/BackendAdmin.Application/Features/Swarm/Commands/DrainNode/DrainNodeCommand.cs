namespace BackendAdmin.Application.Features.Swarm.Commands.DrainNode;

public record DrainNodeCommand(string NodeId) : ICommand<DrainNodeResult>;

public record DrainNodeResult(string Message);
