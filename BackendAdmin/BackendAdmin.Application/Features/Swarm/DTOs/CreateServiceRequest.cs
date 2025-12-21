namespace BackendAdmin.Application.Features.Swarm.DTOs;

public record CreateServiceRequest(
    string Name,
    string Image,
    int Replicas = 1,
    List<CreateServicePortDTO>? Ports = null,
    Dictionary<string, string>? Env = null,
    Dictionary<string, string>? Labels = null
);

public record CreateServicePortDTO(
    int TargetPort,
    int PublishedPort,
    string Protocol = "tcp"
);
