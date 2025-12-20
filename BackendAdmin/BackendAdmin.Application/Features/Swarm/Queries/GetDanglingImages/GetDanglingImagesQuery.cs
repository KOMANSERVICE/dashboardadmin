using BackendAdmin.Application.Features.Swarm.DTOs;

namespace BackendAdmin.Application.Features.Swarm.Queries.GetDanglingImages;

public record GetDanglingImagesQuery() : IQuery<GetDanglingImagesResult>;

public record GetDanglingImagesResult(List<ImageDTO> Images);
