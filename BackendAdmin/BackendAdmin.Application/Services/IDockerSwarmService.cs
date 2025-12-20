using Docker.DotNet.Models;
using BackendAdmin.Application.Features.Swarm.DTOs;

namespace BackendAdmin.Application.Services;

public interface IDockerSwarmService
{
    // Existing methods
    Task<IList<SwarmService>> GetServicesAsync(CancellationToken cancellationToken = default);
    Task<SwarmService?> GetServiceByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<string> GetServiceLogsAsync(string serviceName, int? tail = null, string? since = null, CancellationToken cancellationToken = default);
    Task RestartServiceAsync(string serviceName, CancellationToken cancellationToken = default);
    Task<(int previousReplicas, int newReplicas)> ScaleServiceAsync(string serviceName, int replicas, CancellationToken cancellationToken = default);

    // New methods for nodes
    Task<IList<NodeListResponse>> GetNodesAsync(CancellationToken cancellationToken = default);

    // New methods for service tasks
    Task<IList<TaskResponse>> GetServiceTasksAsync(string serviceName, CancellationToken cancellationToken = default);

    // New methods for service management
    Task<string> CreateServiceAsync(CreateServiceRequest request, CancellationToken cancellationToken = default);
    Task DeleteServiceAsync(string serviceName, CancellationToken cancellationToken = default);
    Task UpdateServiceAsync(string serviceName, UpdateServiceRequest request, CancellationToken cancellationToken = default);
    Task RollbackServiceAsync(string serviceName, CancellationToken cancellationToken = default);
}
