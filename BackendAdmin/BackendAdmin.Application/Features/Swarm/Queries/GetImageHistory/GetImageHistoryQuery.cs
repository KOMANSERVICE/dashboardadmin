using BackendAdmin.Application.Features.Swarm.DTOs;

namespace BackendAdmin.Application.Features.Swarm.Queries.GetImageHistory;

public record GetImageHistoryQuery(string ImageId) : IQuery<GetImageHistoryResult>;

public record GetImageHistoryResult(List<ImageHistoryDTO> History);
