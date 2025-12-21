namespace BackendAdmin.Application.Features.Swarm.DTOs;

/// <summary>
/// DTO representing Docker system information
/// </summary>
public record SystemInfoDTO(
    string Id,
    string Name,
    string OperatingSystem,
    string OSType,
    string Architecture,
    long NCPU,
    long MemTotal,
    string DockerRootDir,
    string KernelVersion,
    long Containers,
    long ContainersRunning,
    long ContainersPaused,
    long ContainersStopped,
    long Images,
    string Driver,
    bool MemoryLimit,
    bool SwapLimit,
    bool CpuCfsPeriod,
    bool CpuCfsQuota,
    bool CPUShares,
    bool IPv4Forwarding,
    bool Debug,
    bool ExperimentalBuild,
    string HttpProxy,
    string HttpsProxy,
    string NoProxy,
    string ServerVersion,
    string ClusterStore,
    DateTime SystemTime,
    string LoggingDriver
);

/// <summary>
/// DTO representing Docker version information
/// </summary>
public record DockerVersionDTO(
    string Version,
    string ApiVersion,
    string MinAPIVersion,
    string GitCommit,
    string GoVersion,
    string Os,
    string Arch,
    string KernelVersion,
    bool Experimental,
    DateTime BuildTime
);

/// <summary>
/// DTO representing Docker disk usage
/// </summary>
public record DiskUsageDTO(
    long LayersSize,
    List<DiskUsageImageDTO> Images,
    List<DiskUsageContainerDTO> Containers,
    List<DiskUsageVolumeDTO> Volumes,
    List<DiskUsageBuildCacheDTO>? BuildCache,
    DiskUsageSummaryDTO Summary
);

/// <summary>
/// DTO for image disk usage
/// </summary>
public record DiskUsageImageDTO(
    string Id,
    List<string> RepoTags,
    long Size,
    long SharedSize,
    long VirtualSize,
    long Containers,
    DateTime Created
);

/// <summary>
/// DTO for container disk usage
/// </summary>
public record DiskUsageContainerDTO(
    string Id,
    string Name,
    string Image,
    string ImageID,
    long SizeRw,
    long SizeRootFs,
    string State,
    DateTime Created
);

/// <summary>
/// DTO for volume disk usage
/// </summary>
public record DiskUsageVolumeDTO(
    string Name,
    string Driver,
    string Mountpoint,
    long Size,
    int UsageCount,
    DateTime? CreatedAt
);

/// <summary>
/// DTO for build cache disk usage
/// </summary>
public record DiskUsageBuildCacheDTO(
    string Id,
    string Type,
    string Description,
    long Size,
    bool InUse,
    bool Shared,
    DateTime CreatedAt,
    DateTime? LastUsedAt
);

/// <summary>
/// DTO for disk usage summary
/// </summary>
public record DiskUsageSummaryDTO(
    long TotalSize,
    long ReclaimableSize,
    int TotalImages,
    int TotalContainers,
    int TotalVolumes,
    int TotalBuildCache,
    long ImagesSize,
    long ContainersSize,
    long VolumesSize,
    long BuildCacheSize
);

/// <summary>
/// Response DTO for prune all operation
/// </summary>
public record PruneAllResponseDTO(
    int ContainersDeleted,
    long ContainersSpaceReclaimed,
    int ImagesDeleted,
    long ImagesSpaceReclaimed,
    int VolumesDeleted,
    long VolumesSpaceReclaimed,
    int NetworksDeleted,
    int BuildCacheDeleted,
    long BuildCacheSpaceReclaimed,
    long TotalSpaceReclaimed,
    List<string> DeletedContainers,
    List<string> DeletedImages,
    List<string> DeletedVolumes,
    List<string> DeletedNetworks
);

/// <summary>
/// DTO for Docker events stream configuration
/// </summary>
public record DockerEventsStreamConfigDTO(
    DateTime? Since,
    DateTime? Until,
    List<string>? Types,
    List<string>? Actions
);
