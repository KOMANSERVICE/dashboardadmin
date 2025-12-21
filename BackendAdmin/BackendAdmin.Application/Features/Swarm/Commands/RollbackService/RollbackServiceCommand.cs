namespace BackendAdmin.Application.Features.Swarm.Commands.RollbackService;

public record RollbackServiceCommand(string ServiceName) : ICommand<RollbackServiceResult>;

public record RollbackServiceResult(string ServiceName);
