using BackendAdmin.Application.Features.Swarm.DTOs;

namespace BackendAdmin.Application.Features.Swarm.Queries.GetServiceTasks;

public record GetServiceTasksQuery(string ServiceName) : IQuery<GetServiceTasksResult>;

public record GetServiceTasksResult(List<ServiceTaskDTO> Tasks);
