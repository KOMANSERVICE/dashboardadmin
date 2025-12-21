using BackendAdmin.Application.Features.Swarm.DTOs;

namespace BackendAdmin.Application.Features.Swarm.Commands.CreateService;

public record CreateServiceCommand(
    string Name,
    string Image,
    int Replicas = 1,
    List<CreateServicePortDTO>? Ports = null,
    Dictionary<string, string>? Env = null,
    Dictionary<string, string>? Labels = null
) : ICommand<CreateServiceResult>;

public record CreateServiceResult(string ServiceId, string ServiceName);
