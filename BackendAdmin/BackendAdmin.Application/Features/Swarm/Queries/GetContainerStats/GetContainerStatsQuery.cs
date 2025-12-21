using BackendAdmin.Application.Features.Swarm.DTOs;

namespace BackendAdmin.Application.Features.Swarm.Queries.GetContainerStats;

public record GetContainerStatsQuery(string ContainerId) : IQuery<GetContainerStatsResult>;

public record GetContainerStatsResult(ContainerStatsDTO Stats);
