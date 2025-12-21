using BackendAdmin.Application.Features.Swarm.DTOs;

namespace BackendAdmin.Application.Features.Swarm.Queries.GetVolumes;

public record GetVolumesQuery() : IQuery<GetVolumesResult>;

public record GetVolumesResult(List<VolumeDTO> Volumes);
