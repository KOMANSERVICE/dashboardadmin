using BackendAdmin.Application.Features.Swarm.DTOs;

namespace BackendAdmin.Application.Features.Swarm.Queries.GetUnusedVolumes;

public record GetUnusedVolumesQuery() : IQuery<GetUnusedVolumesResult>;

public record GetUnusedVolumesResult(List<VolumeDTO> Volumes);
