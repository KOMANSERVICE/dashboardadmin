namespace BackendAdmin.Application.Features.Swarm.DTOs;

/// <summary>
/// DTO pour les ressources et limites d'un service Docker Swarm
/// </summary>
public record ServiceResourcesDTO(
    string ServiceName,
    double? CpuLimit,
    double? CpuReservation,
    string? MemoryLimit,
    string? MemoryReservation,
    long? PidsLimit,
    int? BlkioWeight,
    List<UlimitDTO>? Ulimits,
    DateTime? CreatedAt,
    DateTime? UpdatedAt
);

/// <summary>
/// DTO pour un ulimit
/// </summary>
public record UlimitDTO(
    string Name,
    long Soft,
    long Hard
);

/// <summary>
/// Request pour mettre a jour les ressources d'un service
/// </summary>
public record UpdateServiceResourcesRequest(
    double? CpuLimit = null,
    double? CpuReservation = null,
    string? MemoryLimit = null,
    string? MemoryReservation = null,
    long? PidsLimit = null,
    int? BlkioWeight = null,
    List<UlimitDTO>? Ulimits = null
);

/// <summary>
/// Response apres mise a jour des ressources
/// </summary>
public record UpdateServiceResourcesResponse(
    string ServiceName,
    string Message
);
