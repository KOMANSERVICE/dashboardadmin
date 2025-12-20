namespace FrontendAdmin.Shared.Pages.Swarm.Models;

// Node models
public record NodeDto(
    string Id,
    string Hostname,
    string Role,
    string State,
    string Availability,
    string EngineVersion,
    long NanoCPUs,
    long MemoryBytes,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public record GetNodesResponse(List<NodeDto> Nodes);

// Service models
public record SwarmServiceDto(
    string Id,
    string Name,
    int Replicas,
    int DesiredReplicas,
    string Image,
    string Status,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public record GetSwarmServicesResponse(List<SwarmServiceDto> Services);

// Service details models
public record ServiceDetailsDto(
    string Id,
    string Name,
    int Replicas,
    int DesiredReplicas,
    string Image,
    string Status,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    List<ServicePortDto> Ports,
    Dictionary<string, string> Env,
    Dictionary<string, string> Labels,
    List<string> Networks
);

public record ServicePortDto(
    int TargetPort,
    int PublishedPort,
    string Protocol
);

public record GetServiceDetailsResponse(ServiceDetailsDto Service);

// Service tasks models
public record ServiceTaskDto(
    string Id,
    string NodeId,
    string State,
    string? ContainerId,
    string? Message,
    DateTime CreatedAt,
    DateTime UpdatedAt
);

public record GetServiceTasksResponse(List<ServiceTaskDto> Tasks);

// Service logs models
public record GetServiceLogsResponse(
    string ServiceName,
    string Logs,
    DateTime FetchedAt
);

// Create service models
public record CreateServicePortRequest(
    int TargetPort,
    int PublishedPort,
    string Protocol = "tcp"
);

public record CreateServiceRequest(
    string Name,
    string Image,
    int Replicas = 1,
    List<CreateServicePortRequest>? Ports = null,
    Dictionary<string, string>? Env = null,
    Dictionary<string, string>? Labels = null
);

public record CreateServiceResponse(string ServiceId, string ServiceName);

// Update service models
public record UpdateServiceRequest(
    string? Image = null,
    int? Replicas = null,
    Dictionary<string, string>? Env = null,
    Dictionary<string, string>? Labels = null
);

public record UpdateServiceResponse(string ServiceName);

// Scale service models
public record ScaleServiceRequest(int Replicas);

public record ScaleServiceResponse(
    string ServiceName,
    int PreviousReplicas,
    int NewReplicas
);

// Rollback service models
public record RollbackServiceResponse(string ServiceName);
