using Docker.DotNet.Models;

namespace BackendAdmin.Application.Services;

public interface IDockerSwarmService
{
    Task<IList<SwarmService>> GetServicesAsync(CancellationToken cancellationToken = default);
    Task<SwarmService?> GetServiceByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<string> GetServiceLogsAsync(string serviceName, int? tail = null, string? since = null, CancellationToken cancellationToken = default);
    Task RestartServiceAsync(string serviceName, CancellationToken cancellationToken = default);
    Task<(int previousReplicas, int newReplicas)> ScaleServiceAsync(string serviceName, int replicas, CancellationToken cancellationToken = default);
}
