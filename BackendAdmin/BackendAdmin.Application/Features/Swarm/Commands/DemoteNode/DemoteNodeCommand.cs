namespace BackendAdmin.Application.Features.Swarm.Commands.DemoteNode;

public record DemoteNodeCommand(string NodeId) : ICommand<DemoteNodeResult>;

public record DemoteNodeResult(string Message);
