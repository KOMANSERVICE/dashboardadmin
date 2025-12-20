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

// Volume models
public record VolumeDto(
    string Name,
    string Driver,
    string Mountpoint,
    long SizeBytes,
    DateTime CreatedAt,
    Dictionary<string, string> Labels,
    bool IsUsed
);

public record VolumeDetailsDto(
    string Name,
    string Driver,
    string Mountpoint,
    long SizeBytes,
    DateTime CreatedAt,
    Dictionary<string, string> Labels,
    bool IsUsed,
    Dictionary<string, string> Options,
    string Scope,
    List<string> UsedByContainers
);

public record GetVolumesResponse(List<VolumeDto> Volumes);

public record GetVolumeDetailsResponse(VolumeDetailsDto Volume);

public record GetUnusedVolumesResponse(List<VolumeDto> Volumes);

public record CreateVolumeRequest(
    string Name,
    string Driver = "local",
    Dictionary<string, string>? Labels = null,
    Dictionary<string, string>? DriverOpts = null
);

public record CreateVolumeResponse(string VolumeName);

public record PruneVolumesResponse(
    int DeletedCount,
    long SpaceReclaimed,
    List<string> DeletedVolumes
);

public record BackupVolumeRequest(string DestinationPath);

public record BackupVolumeResponse(
    string VolumeName,
    string BackupPath,
    DateTime BackupDate,
    long SizeBytes
);

public record RestoreVolumeRequest(string SourcePath);

public record RestoreVolumeResponse(
    string VolumeName,
    string SourcePath,
    DateTime RestoreDate
);
