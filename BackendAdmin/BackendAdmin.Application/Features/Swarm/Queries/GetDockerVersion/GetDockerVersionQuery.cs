using BackendAdmin.Application.Features.Swarm.DTOs;

namespace BackendAdmin.Application.Features.Swarm.Queries.GetDockerVersion;

public record GetDockerVersionQuery() : IQuery<GetDockerVersionResult>;

public record GetDockerVersionResult(DockerVersionDTO Version);
