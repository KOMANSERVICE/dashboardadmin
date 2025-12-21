namespace BackendAdmin.Application.Features.Swarm.DTOs;

public record ServiceTaskDTO(
    string Id,
    string NodeId,
    string State,
    string? ContainerId,
    string? Message,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
