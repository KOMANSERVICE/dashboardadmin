namespace BackendAdmin.Application.Features.Swarm.Commands.PromoteNode;

public record PromoteNodeCommand(string NodeId) : ICommand<PromoteNodeResult>;

public record PromoteNodeResult(string Message);
