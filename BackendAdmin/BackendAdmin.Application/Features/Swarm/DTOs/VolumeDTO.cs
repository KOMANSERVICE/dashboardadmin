namespace BackendAdmin.Application.Features.Swarm.DTOs;

/// <summary>
/// DTO representing a Docker volume
/// </summary>
public record VolumeDTO(
    string Name,
    string Driver,
    string Mountpoint,
    long SizeBytes,
    DateTime CreatedAt,
    IDictionary<string, string> Labels,
    bool IsUsed
);

/// <summary>
/// DTO representing detailed volume information
/// </summary>
public record VolumeDetailsDTO(
    string Name,
    string Driver,
    string Mountpoint,
    long SizeBytes,
    DateTime CreatedAt,
    IDictionary<string, string> Labels,
    bool IsUsed,
    IDictionary<string, string> Options,
    string Scope,
    List<string> UsedByContainers
);

/// <summary>
/// Request DTO for creating a new volume
/// </summary>
public record CreateVolumeRequest(
    string Name,
    string Driver = "local",
    Dictionary<string, string>? Labels = null,
    Dictionary<string, string>? DriverOpts = null
);

/// <summary>
/// Response DTO for prune volumes operation
/// </summary>
public record PruneVolumesResponse(
    int DeletedCount,
    long SpaceReclaimed,
    List<string> DeletedVolumes
);

/// <summary>
/// Request DTO for backing up a volume
/// </summary>
public record BackupVolumeRequest(
    string DestinationPath
);

/// <summary>
/// Response DTO for backup volume operation
/// </summary>
public record BackupVolumeResponse(
    string VolumeName,
    string BackupPath,
    DateTime BackupDate,
    long SizeBytes
);

/// <summary>
/// Request DTO for restoring a volume
/// </summary>
public record RestoreVolumeRequest(
    string SourcePath
);

/// <summary>
/// Response DTO for restore volume operation
/// </summary>
public record RestoreVolumeResponse(
    string VolumeName,
    string SourcePath,
    DateTime RestoreDate
);
