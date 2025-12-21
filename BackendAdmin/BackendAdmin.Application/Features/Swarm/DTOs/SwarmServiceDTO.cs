namespace BackendAdmin.Application.Features.Swarm.DTOs;

public record SwarmServiceDTO(
    string Id,
    string Name,
    int Replicas,
    int DesiredReplicas,
    string Image,
    string Status,
    DateTime CreatedAt,
    DateTime UpdatedAt
);
