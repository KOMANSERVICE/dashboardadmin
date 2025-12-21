namespace BackendAdmin.Application.Features.Swarm.Commands.UpdateService;

public record UpdateServiceCommand(
    string ServiceName,
    string? Image = null,
    int? Replicas = null,
    Dictionary<string, string>? Env = null,
    Dictionary<string, string>? Labels = null
) : ICommand<UpdateServiceResult>;

public record UpdateServiceResult(string ServiceName);
