using BackendAdmin.Application.Features.Swarm.DTOs;

namespace BackendAdmin.Application.Features.Swarm.Queries.GetDiskUsage;

public record GetDiskUsageQuery() : IQuery<GetDiskUsageResult>;

public record GetDiskUsageResult(DiskUsageDTO DiskUsage);
