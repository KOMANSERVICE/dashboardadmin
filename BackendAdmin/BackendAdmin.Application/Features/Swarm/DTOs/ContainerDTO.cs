namespace BackendAdmin.Application.Features.Swarm.DTOs;

/// <summary>
/// DTO representing a Docker container
/// </summary>
public record ContainerDTO(
    string Id,
    string Name,
    string Image,
    string State,
    string Status,
    DateTime CreatedAt
);

/// <summary>
/// DTO representing detailed container information
/// </summary>
public record ContainerDetailsDTO(
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
    List<ContainerMountDTO> Mounts,
    List<ContainerNetworkDTO> Networks,
    List<ContainerPortDTO> Ports,
    ContainerHostConfigDTO HostConfig
);

/// <summary>
/// DTO for container mount information
/// </summary>
public record ContainerMountDTO(
    string Type,
    string Source,
    string Destination,
    bool ReadOnly
);

/// <summary>
/// DTO for container network information
/// </summary>
public record ContainerNetworkDTO(
    string NetworkId,
    string NetworkName,
    string? IpAddress,
    string? Gateway,
    string? MacAddress
);

/// <summary>
/// DTO for container port mapping
/// </summary>
public record ContainerPortDTO(
    int PrivatePort,
    int? PublicPort,
    string Type,
    string? Ip
);

/// <summary>
/// DTO for container host configuration
/// </summary>
public record ContainerHostConfigDTO(
    long Memory,
    long MemorySwap,
    long CpuShares,
    long NanoCpus,
    string RestartPolicy,
    bool Privileged,
    bool ReadonlyRootfs
);

/// <summary>
/// DTO for container statistics
/// </summary>
public record ContainerStatsDTO(
    string ContainerId,
    string ContainerName,
    DateTime ReadAt,
    ContainerCpuStatsDTO Cpu,
    ContainerMemoryStatsDTO Memory,
    ContainerNetworkStatsDTO Network,
    ContainerBlockIOStatsDTO BlockIO
);

/// <summary>
/// DTO for CPU statistics
/// </summary>
public record ContainerCpuStatsDTO(
    double UsagePercent,
    ulong TotalUsage,
    ulong SystemUsage,
    int OnlineCpus
);

/// <summary>
/// DTO for memory statistics
/// </summary>
public record ContainerMemoryStatsDTO(
    ulong Usage,
    ulong MaxUsage,
    ulong Limit,
    double UsagePercent
);

/// <summary>
/// DTO for network statistics
/// </summary>
public record ContainerNetworkStatsDTO(
    ulong RxBytes,
    ulong TxBytes,
    ulong RxPackets,
    ulong TxPackets
);

/// <summary>
/// DTO for block I/O statistics
/// </summary>
public record ContainerBlockIOStatsDTO(
    ulong ReadBytes,
    ulong WriteBytes
);

/// <summary>
/// DTO for container logs
/// </summary>
public record ContainerLogsDTO(
    string ContainerId,
    string ContainerName,
    string Logs,
    DateTime FetchedAt
);

/// <summary>
/// Request DTO for executing a command in a container
/// </summary>
public record ContainerExecRequest(
    string Command,
    string[]? Args = null,
    bool AttachStdout = true,
    bool AttachStderr = true,
    string? WorkingDir = null,
    Dictionary<string, string>? Env = null
);

/// <summary>
/// Response DTO for container exec operation
/// </summary>
public record ContainerExecResponse(
    string ContainerId,
    string ContainerName,
    string Command,
    int ExitCode,
    string Stdout,
    string Stderr,
    DateTime ExecutedAt
);

/// <summary>
/// DTO for container process information (top)
/// </summary>
public record ContainerProcessDTO(
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

/// <summary>
/// DTO for container top response
/// </summary>
public record ContainerTopDTO(
    string ContainerId,
    string ContainerName,
    List<string> Titles,
    List<ContainerProcessDTO> Processes
);

/// <summary>
/// DTO for filesystem change
/// </summary>
public record FilesystemChangeDTO(
    string Path,
    string Kind
);

/// <summary>
/// DTO for container filesystem changes response
/// </summary>
public record ContainerChangesDTO(
    string ContainerId,
    string ContainerName,
    List<FilesystemChangeDTO> Changes
);

/// <summary>
/// DTO for container size information
/// </summary>
public record ContainerSizeDTO(
    string ContainerId,
    string ContainerName,
    long SizeRootFs,
    long SizeRw
);
