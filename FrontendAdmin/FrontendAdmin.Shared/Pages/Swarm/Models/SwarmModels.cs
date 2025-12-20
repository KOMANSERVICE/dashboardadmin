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

// Container models
public record ContainerDto(
    string Id,
    string Name,
    string Image,
    string State,
    string Status,
    DateTime CreatedAt
);

public record GetContainersResponse(List<ContainerDto> Containers);

public record ContainerDetailsDto(
    string Id,
    string Name,
    string Image,
    string State,
    string Status,
    DateTime CreatedAt,
    string Platform,
    string Driver,
    long SizeRootFs,
    long SizeRw,
    string? Command,
    string? WorkingDir,
    Dictionary<string, string> Labels,
    Dictionary<string, string> Env,
    List<ContainerMountDto> Mounts,
    List<ContainerNetworkDto> Networks,
    List<ContainerPortDto> Ports,
    ContainerHostConfigDto HostConfig
);

public record ContainerMountDto(
    string Type,
    string Source,
    string Destination,
    bool ReadOnly
);

public record ContainerNetworkDto(
    string NetworkId,
    string NetworkName,
    string? IpAddress,
    string? Gateway,
    string? MacAddress
);

public record ContainerPortDto(
    int PrivatePort,
    int? PublicPort,
    string Type,
    string? Ip
);

public record ContainerHostConfigDto(
    long Memory,
    long MemorySwap,
    long CpuShares,
    long NanoCpus,
    string RestartPolicy,
    bool Privileged,
    bool ReadonlyRootfs
);

public record GetContainerInspectResponse(ContainerDetailsDto Details);

// Container stats models
public record ContainerStatsDto(
    string ContainerId,
    string ContainerName,
    DateTime ReadAt,
    ContainerCpuStatsDto Cpu,
    ContainerMemoryStatsDto Memory,
    ContainerNetworkStatsDto Network,
    ContainerBlockIOStatsDto BlockIO
);

public record ContainerCpuStatsDto(
    double UsagePercent,
    ulong TotalUsage,
    ulong SystemUsage,
    int OnlineCpus
);

public record ContainerMemoryStatsDto(
    ulong Usage,
    ulong MaxUsage,
    ulong Limit,
    double UsagePercent
);

public record ContainerNetworkStatsDto(
    ulong RxBytes,
    ulong TxBytes,
    ulong RxPackets,
    ulong TxPackets
);

public record ContainerBlockIOStatsDto(
    ulong ReadBytes,
    ulong WriteBytes
);

public record GetContainerStatsResponse(ContainerStatsDto Stats);

// Container logs models
public record ContainerLogsDto(
    string ContainerId,
    string ContainerName,
    string Logs,
    DateTime FetchedAt
);

public record GetContainerLogsResponse(ContainerLogsDto Logs);

// Container exec models
public record ContainerExecRequest(
    string Command,
    string[]? Args = null,
    bool AttachStdout = true,
    bool AttachStderr = true,
    string? WorkingDir = null,
    Dictionary<string, string>? Env = null
);

public record ContainerExecResponseDto(
    string ContainerId,
    string ContainerName,
    string Command,
    int ExitCode,
    string Stdout,
    string Stderr,
    DateTime ExecutedAt
);

public record ExecContainerResponse(ContainerExecResponseDto Response);

// Container top models
public record ContainerProcessDto(
    string Pid,
    string User,
    string Cpu,
    string Memory,
    string Vsz,
    string Rss,
    string Tty,
    string Stat,
    string Start,
    string Time,
    string Command
);

public record ContainerTopDto(
    string ContainerId,
    string ContainerName,
    List<string> Titles,
    List<ContainerProcessDto> Processes
);

public record GetContainerTopResponse(ContainerTopDto Top);

// Container changes models
public record FilesystemChangeDto(
    string Path,
    string Kind
);

public record ContainerChangesDto(
    string ContainerId,
    string ContainerName,
    List<FilesystemChangeDto> Changes
);

public record GetContainerChangesResponse(ContainerChangesDto Changes);

// Container size models
public record ContainerSizeDto(
    string ContainerId,
    string ContainerName,
    long SizeRootFs,
    long SizeRw
);

public record GetContainerSizeResponse(ContainerSizeDto Size);

// Service Resources models
public record ServiceResourcesDto(
    string ServiceName,
    double? CpuLimit,
    double? CpuReservation,
    string? MemoryLimit,
    string? MemoryReservation,
    long? PidsLimit,
    int? BlkioWeight,
    List<UlimitDto>? Ulimits,
    DateTime? CreatedAt,
    DateTime? UpdatedAt
);

public record UlimitDto(
    string Name,
    long Soft,
    long Hard
);

public record GetServiceResourcesResponse(ServiceResourcesDto Resources);

public record UpdateServiceResourcesRequest(
    double? CpuLimit = null,
    double? CpuReservation = null,
    string? MemoryLimit = null,
    string? MemoryReservation = null,
    long? PidsLimit = null,
    int? BlkioWeight = null,
    List<UlimitDto>? Ulimits = null
);

public record UpdateServiceResourcesResponse(
    string ServiceName,
    string Message
);

// Network models
public record NetworkDto(
    string Id,
    string Name,
    string Driver,
    string Scope,
    bool IsInternal,
    bool IsAttachable,
    DateTime CreatedAt,
    int ContainerCount
);

public record NetworkDetailsDto(
    string Id,
    string Name,
    string Driver,
    string Scope,
    bool IsInternal,
    bool IsAttachable,
    DateTime CreatedAt,
    string? Subnet,
    string? Gateway,
    string? IpRange,
    Dictionary<string, string> Labels,
    Dictionary<string, string> Options,
    List<NetworkContainerDto> Containers
);

public record NetworkContainerDto(
    string ContainerId,
    string ContainerName,
    string? IpAddress,
    string? MacAddress
);

public record GetNetworksResponse(List<NetworkDto> Networks);

public record GetNetworkDetailsResponse(NetworkDetailsDto Network);

public record CreateNetworkRequest(
    string Name,
    string Driver = "overlay",
    bool IsAttachable = true,
    string? Subnet = null,
    string? Gateway = null,
    string? IpRange = null,
    Dictionary<string, string>? Labels = null,
    Dictionary<string, string>? Options = null
);

public record CreateNetworkResponse(string NetworkId, string NetworkName);

public record PruneNetworksResponse(
    int DeletedCount,
    List<string> DeletedNetworks
);

public record ConnectContainerRequest(
    string ContainerId,
    string? IpAddress = null
);

public record ConnectContainerResponse(
    bool Success,
    string NetworkName,
    string ContainerId
);

public record DisconnectContainerRequest(
    string ContainerId,
    bool Force = false
);

public record DisconnectContainerResponse(
    bool Success,
    string NetworkName,
    string ContainerId
);
