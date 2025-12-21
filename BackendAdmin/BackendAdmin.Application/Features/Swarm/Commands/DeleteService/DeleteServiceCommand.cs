namespace BackendAdmin.Application.Features.Swarm.Commands.DeleteService;

public record DeleteServiceCommand(string ServiceName) : ICommand<DeleteServiceResult>;

public record DeleteServiceResult(string ServiceName);
