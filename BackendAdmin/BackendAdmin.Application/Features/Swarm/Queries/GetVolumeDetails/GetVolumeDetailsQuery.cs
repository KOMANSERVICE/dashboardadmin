using BackendAdmin.Application.Features.Swarm.DTOs;

namespace BackendAdmin.Application.Features.Swarm.Queries.GetVolumeDetails;

public record GetVolumeDetailsQuery(string Name) : IQuery<GetVolumeDetailsResult>;

public record GetVolumeDetailsResult(VolumeDetailsDTO Volume);
