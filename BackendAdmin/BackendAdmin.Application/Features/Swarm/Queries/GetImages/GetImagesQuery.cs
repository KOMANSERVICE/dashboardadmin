using BackendAdmin.Application.Features.Swarm.DTOs;

namespace BackendAdmin.Application.Features.Swarm.Queries.GetImages;

public record GetImagesQuery(bool All = false) : IQuery<GetImagesResult>;

public record GetImagesResult(List<ImageDTO> Images);
