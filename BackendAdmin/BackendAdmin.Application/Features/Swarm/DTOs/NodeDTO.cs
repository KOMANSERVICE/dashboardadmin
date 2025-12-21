namespace BackendAdmin.Application.Features.Swarm.DTOs;

public record NodeDTO(
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
