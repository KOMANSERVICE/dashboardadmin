namespace BackendAdmin.Application.Features.Swarm.DTOs;

public record ServiceDetailsDTO(
    string Id,
    string Name,
    int Replicas,
    int DesiredReplicas,
    string Image,
    string Status,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    List<ServicePortDTO> Ports,
    Dictionary<string, string> Env,
    Dictionary<string, string> Labels,
    List<string> Networks
);

public record ServicePortDTO(
    int TargetPort,
    int PublishedPort,
    string Protocol
);
