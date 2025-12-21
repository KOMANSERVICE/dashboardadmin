namespace BackendAdmin.Application.Features.Swarm.DTOs;

/// <summary>
/// DTO representing a Docker Stack
/// </summary>
public record StackDTO(
    string Name,
    int ServiceCount,
    string Orchestrator,
    DateTime CreatedAt
);

/// <summary>
/// DTO representing detailed stack information
/// </summary>
public record StackDetailsDTO(
    string Name,
    int ServiceCount,
    string Orchestrator,
    DateTime CreatedAt,
    List<StackServiceDTO> Services
);

/// <summary>
/// DTO representing a service within a stack
/// </summary>
public record StackServiceDTO(
    string Id,
    string Name,
    string Image,
    int Replicas,
    int DesiredReplicas,
    string Status,
    List<StackServicePortDTO> Ports
);

/// <summary>
/// DTO representing a port mapping in a stack service
/// </summary>
public record StackServicePortDTO(
    int TargetPort,
    int PublishedPort,
    string Protocol
);

/// <summary>
/// Request DTO for deploying a new stack
/// </summary>
public record DeployStackRequest(
    string Name,
    string ComposeFileContent,
    bool Prune = false
);

/// <summary>
/// Response DTO for deploy stack operation
/// </summary>
public record DeployStackResponse(
    string StackName,
    int ServicesDeployed,
    DateTime DeployedAt
);
