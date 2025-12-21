using BackendAdmin.Application.Features.Swarm.DTOs;
using BackendAdmin.Application.Services;

namespace BackendAdmin.Application.Features.Swarm.Queries.GetImageHistory;

public class GetImageHistoryHandler(IDockerSwarmService dockerSwarmService)
    : IQueryHandler<GetImageHistoryQuery, GetImageHistoryResult>
{
    public async Task<GetImageHistoryResult> Handle(GetImageHistoryQuery request, CancellationToken cancellationToken)
    {
        var history = await dockerSwarmService.GetImageHistoryAsync(request.ImageId, cancellationToken);

        var historyDtos = history.Select(h => new ImageHistoryDTO(
            Id: h.ID.Replace("sha256:", ""),
            CreatedBy: h.CreatedBy,
            CreatedAt: h.Created,
            Size: h.Size,
            Comment: h.Comment,
            Tags: h.Tags?.ToList() ?? new List<string>()
        )).ToList();

        return new GetImageHistoryResult(historyDtos);
    }
}
