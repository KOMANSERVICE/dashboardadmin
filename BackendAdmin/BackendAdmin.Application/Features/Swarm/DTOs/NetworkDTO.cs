namespace BackendAdmin.Application.Features.Swarm.DTOs;

/// <summary>
/// DTO representing a Docker network
/// </summary>
public record NetworkDTO(
    string Id,
    string Name,
    string Driver,
    string Scope,
    bool IsInternal,
    bool IsAttachable,
    DateTime CreatedAt,
    int ContainerCount
);

/// <summary>
/// DTO representing detailed network information
/// </summary>
public record NetworkDetailsDTO(
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
    IDictionary<string, string> Labels,
    IDictionary<string, string> Options,
    List<NetworkContainerDTO> Containers
);

/// <summary>
/// DTO representing a container connected to a network
/// </summary>
public record NetworkContainerDTO(
    string ContainerId,
    string ContainerName,
    string? IpAddress,
    string? MacAddress
);

/// <summary>
/// Request DTO for creating a new network
/// </summary>
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

/// <summary>
/// Request DTO for connecting a container to a network
/// </summary>
public record ConnectContainerRequest(
    string ContainerId,
    string? IpAddress = null
);

/// <summary>
/// Request DTO for disconnecting a container from a network
/// </summary>
public record DisconnectContainerRequest(
    string ContainerId,
    bool Force = false
);

/// <summary>
/// Response DTO for prune networks operation
/// </summary>
public record PruneNetworksResponse(
    int DeletedCount,
    List<string> DeletedNetworks
);
