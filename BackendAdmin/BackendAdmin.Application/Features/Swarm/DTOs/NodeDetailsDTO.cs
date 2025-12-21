namespace BackendAdmin.Application.Features.Swarm.DTOs;

public record NodeDetailsDTO(
    string Id,
    string Hostname,
    string Role,
    string State,
    string Availability,
    string EngineVersion,
    long NanoCPUs,
    long MemoryBytes,
    DateTime CreatedAt,
    DateTime UpdatedAt,
    string? StatusMessage,
    string? StatusAddr,
    string? Platform,
    string? Architecture,
    Dictionary<string, string> Labels
);
