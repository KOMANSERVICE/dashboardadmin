namespace BackendAdmin.Application.Features.Swarm.DTOs;

public record UpdateServiceRequest(
    string? Image = null,
    int? Replicas = null,
    Dictionary<string, string>? Env = null,
    Dictionary<string, string>? Labels = null
);
