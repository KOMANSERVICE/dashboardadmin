namespace BackendAdmin.Application.Features.Swarm.Commands.RemoveNode;

public record RemoveNodeCommand(string NodeId, bool Force = false) : ICommand<RemoveNodeResult>;

public record RemoveNodeResult(string Message);
