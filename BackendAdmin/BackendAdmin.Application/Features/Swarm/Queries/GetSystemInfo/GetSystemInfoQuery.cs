using BackendAdmin.Application.Features.Swarm.DTOs;

namespace BackendAdmin.Application.Features.Swarm.Queries.GetSystemInfo;

public record GetSystemInfoQuery() : IQuery<GetSystemInfoResult>;

public record GetSystemInfoResult(SystemInfoDTO SystemInfo);
