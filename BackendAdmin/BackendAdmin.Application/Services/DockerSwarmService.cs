using Docker.DotNet;
using Docker.DotNet.Models;
using Microsoft.Extensions.Logging;

namespace BackendAdmin.Application.Services;

public class DockerSwarmService : IDockerSwarmService
{
    private readonly ILogger<DockerSwarmService> _logger;
    private readonly DockerClient _client;

    public DockerSwarmService(ILogger<DockerSwarmService> logger)
    {
        _logger = logger;
        _client = new DockerClientConfiguration(
            new Uri("unix:///var/run/docker.sock"))
            .CreateClient();
    }

    public async Task<IList<SwarmService>> GetServicesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var services = await _client.Swarm.ListServicesAsync(null, cancellationToken);
            return services.ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get Docker Swarm services");
            throw new InternalServerException("Docker socket non accessible");
        }
    }

    public async Task<SwarmService?> GetServiceByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        var services = await GetServicesAsync(cancellationToken);
        return services.FirstOrDefault(s => s.Spec.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
    }

    public async Task<string> GetServiceLogsAsync(string serviceName, int? tail = null, string? since = null, CancellationToken cancellationToken = default)
    {
        var service = await GetServiceByNameAsync(serviceName, cancellationToken);
        if (service == null)
        {
            throw new NotFoundException($"Service '{serviceName}' non trouve");
        }

        try
        {
            var parameters = new ServiceLogsParameters
            {
                ShowStdout = true,
                ShowStderr = true,
                Timestamps = true
            };

            if (tail.HasValue)
            {
                parameters.Tail = tail.Value.ToString();
            }

            if (!string.IsNullOrEmpty(since))
            {
                parameters.Since = since;
            }

            using var logsStream = await _client.Swarm.GetServiceLogsAsync(service.ID, false, parameters, cancellationToken);
            var logs = await logsStream.ReadOutputToEndAsync(cancellationToken);
            var combinedLogs = logs.stdout + logs.stderr;

            return CleanDockerLogs(combinedLogs);
        }
        catch (NotFoundException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get logs for service {ServiceName}", serviceName);
            throw new InternalServerException($"Erreur lors de la recuperation des logs du service '{serviceName}'");
        }
    }

    public async Task RestartServiceAsync(string serviceName, CancellationToken cancellationToken = default)
    {
        var service = await GetServiceByNameAsync(serviceName, cancellationToken);
        if (service == null)
        {
            throw new NotFoundException($"Service '{serviceName}' non trouve");
        }

        try
        {
            var spec = service.Spec;
            spec.TaskTemplate.ForceUpdate = spec.TaskTemplate.ForceUpdate + 1;

            var updateParams = new ServiceUpdateParameters
            {
                Service = spec,
                Version = (long)service.Version.Index
            };

            await _client.Swarm.UpdateServiceAsync(service.ID, updateParams, cancellationToken);
            _logger.LogInformation("Service {ServiceName} restarted successfully", serviceName);
        }
        catch (NotFoundException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to restart service {ServiceName}", serviceName);
            throw new InternalServerException($"Erreur lors du redemarrage du service '{serviceName}'");
        }
    }

    public async Task<(int previousReplicas, int newReplicas)> ScaleServiceAsync(string serviceName, int replicas, CancellationToken cancellationToken = default)
    {
        var service = await GetServiceByNameAsync(serviceName, cancellationToken);
        if (service == null)
        {
            throw new NotFoundException($"Service '{serviceName}' non trouve");
        }

        try
        {
            var spec = service.Spec;

            if (spec.Mode.Replicated == null)
            {
                throw new BadRequestException($"Le service '{serviceName}' n'est pas en mode replicated et ne peut pas etre scale");
            }

            var previousReplicas = (int)(spec.Mode.Replicated.Replicas ?? 0UL);
            spec.Mode.Replicated.Replicas = (ulong)replicas;

            var updateParams = new ServiceUpdateParameters
            {
                Service = spec,
                Version = (long)service.Version.Index
            };

            await _client.Swarm.UpdateServiceAsync(service.ID, updateParams, cancellationToken);
            _logger.LogInformation("Service {ServiceName} scaled from {PreviousReplicas} to {NewReplicas}", serviceName, previousReplicas, replicas);

            return (previousReplicas, replicas);
        }
        catch (NotFoundException)
        {
            throw;
        }
        catch (BadRequestException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to scale service {ServiceName}", serviceName);
            throw new InternalServerException($"Erreur lors du scaling du service '{serviceName}'");
        }
    }

    private static string CleanDockerLogs(string logs)
    {
        if (string.IsNullOrEmpty(logs))
            return string.Empty;

        var lines = logs.Split('\n');
        var cleanedLines = new List<string>();

        foreach (var line in lines)
        {
            if (line.Length > 8)
            {
                cleanedLines.Add(line[8..]);
            }
            else if (!string.IsNullOrWhiteSpace(line))
            {
                cleanedLines.Add(line);
            }
        }

        return string.Join("\n", cleanedLines);
    }
}
