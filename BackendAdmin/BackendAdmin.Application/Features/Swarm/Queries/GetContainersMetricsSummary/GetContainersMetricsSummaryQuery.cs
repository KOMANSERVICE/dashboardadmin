using BackendAdmin.Application.Features.Swarm.DTOs;

namespace BackendAdmin.Application.Features.Swarm.Queries.GetContainersMetricsSummary;

public record GetContainersMetricsSummaryQuery() : IQuery<GetContainersMetricsSummaryResult>;

public record GetContainersMetricsSummaryResult(IList<ContainerMetricsSummaryDTO> Containers);
