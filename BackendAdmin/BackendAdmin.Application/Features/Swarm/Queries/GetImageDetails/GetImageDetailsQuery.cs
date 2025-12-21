using BackendAdmin.Application.Features.Swarm.DTOs;

namespace BackendAdmin.Application.Features.Swarm.Queries.GetImageDetails;

public record GetImageDetailsQuery(string ImageId) : IQuery<GetImageDetailsResult>;

public record GetImageDetailsResult(ImageDetailsDTO Image);
