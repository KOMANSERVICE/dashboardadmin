namespace BackendAdmin.Application.Features.Swarm.Commands.ActivateNode;

public record ActivateNodeCommand(string NodeId) : ICommand<ActivateNodeResult>;

public record ActivateNodeResult(string Message);
