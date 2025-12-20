using BackendAdmin.Application.Features.Swarm.DTOs;

namespace BackendAdmin.Application.Features.Swarm.Commands.UpdateServiceResources;

public record UpdateServiceResourcesCommand(
    string ServiceName,
    double? CpuLimit = null,
    double? CpuReservation = null,
    string? MemoryLimit = null,
    string? MemoryReservation = null,
    long? PidsLimit = null,
    int? BlkioWeight = null,
    List<UlimitDTO>? Ulimits = null
) : ICommand<UpdateServiceResourcesResult>;

public record UpdateServiceResourcesResult(
    string ServiceName,
    string Message
);
