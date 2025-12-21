namespace BackendAdmin.Application.Features.Swarm.Commands.RestartService;

public record RestartServiceCommand(string ServiceName) : ICommand<RestartServiceResult>;

public record RestartServiceResult(string Message);
