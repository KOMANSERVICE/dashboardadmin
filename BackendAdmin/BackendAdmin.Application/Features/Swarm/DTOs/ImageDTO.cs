namespace BackendAdmin.Application.Features.Swarm.DTOs;

/// <summary>
/// DTO representing a Docker image
/// </summary>
public record ImageDTO(
    string Id,
    string? Repository,
    string? Tag,
    long Size,
    DateTime CreatedAt,
    int ContainerCount,
    bool IsDangling
);

/// <summary>
/// DTO representing detailed image information
/// </summary>
public record ImageDetailsDTO(
    string Id,
    string? Repository,
    string? Tag,
    long Size,
    DateTime CreatedAt,
    string? Author,
    string? Architecture,
    string? Os,
    string? DockerVersion,
    IDictionary<string, string> Labels,
    List<string> RepoTags,
    List<string> RepoDigests,
    string? Comment,
    string? Container,
    ImageConfigDTO Config
);

/// <summary>
/// DTO representing image configuration
/// </summary>
public record ImageConfigDTO(
    string? Hostname,
    string? Domainname,
    string? User,
    List<string>? Cmd,
    List<string>? Entrypoint,
    List<string>? Env,
    string? WorkingDir,
    List<string>? ExposedPorts,
    List<string>? Volumes
);

/// <summary>
/// DTO representing image history (layer)
/// </summary>
public record ImageHistoryDTO(
    string Id,
    string CreatedBy,
    DateTime CreatedAt,
    long Size,
    string? Comment,
    List<string> Tags
);

/// <summary>
/// Request DTO for pulling an image
/// </summary>
public record PullImageRequest(
    string Image,
    string? Tag = "latest",
    string? Registry = null
);

/// <summary>
/// Response DTO for pull image operation
/// </summary>
public record PullImageResponse(
    string ImageName,
    string Tag,
    string Status,
    DateTime PulledAt
);

/// <summary>
/// Request DTO for tagging an image
/// </summary>
public record TagImageRequest(
    string NewRepository,
    string NewTag
);

/// <summary>
/// Response DTO for tag image operation
/// </summary>
public record TagImageResponse(
    string SourceImage,
    string NewRepository,
    string NewTag
);

/// <summary>
/// Request DTO for pushing an image
/// </summary>
public record PushImageRequest(
    string? Tag = null,
    string? Registry = null
);

/// <summary>
/// Response DTO for push image operation
/// </summary>
public record PushImageResponse(
    string ImageName,
    string Tag,
    string Status,
    DateTime PushedAt
);

/// <summary>
/// Response DTO for prune images operation
/// </summary>
public record PruneImagesResponse(
    int DeletedCount,
    long SpaceReclaimed,
    List<string> DeletedImages
);
