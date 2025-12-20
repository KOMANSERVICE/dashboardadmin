using Docker.DotNet;
using Docker.DotNet.Models;
using Microsoft.Extensions.Logging;
using BackendAdmin.Application.Features.Swarm.DTOs;

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

    public async Task<IList<NodeListResponse>> GetNodesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var nodes = await _client.Swarm.ListNodesAsync(cancellationToken);
            return nodes.ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get Docker Swarm nodes");
            throw new InternalServerException("Docker socket non accessible ou Swarm non initialise");
        }
    }

    public async Task<IList<TaskResponse>> GetServiceTasksAsync(string serviceName, CancellationToken cancellationToken = default)
    {
        var service = await GetServiceByNameAsync(serviceName, cancellationToken);
        if (service == null)
        {
            throw new NotFoundException($"Service '{serviceName}' non trouve");
        }

        try
        {
            var tasksListParameters = new TasksListParameters
            {
                Filters = new Dictionary<string, IDictionary<string, bool>>
                {
                    ["service"] = new Dictionary<string, bool> { [service.ID] = true }
                }
            };

            var tasks = await _client.Tasks.ListAsync(tasksListParameters, cancellationToken);
            return tasks.ToList();
        }
        catch (NotFoundException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get tasks for service {ServiceName}", serviceName);
            throw new InternalServerException($"Erreur lors de la recuperation des taches du service '{serviceName}'");
        }
    }

    public async Task<string> CreateServiceAsync(CreateServiceRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            // Check if service already exists
            var existingService = await GetServiceByNameAsync(request.Name, cancellationToken);
            if (existingService != null)
            {
                throw new BadRequestException($"Le service '{request.Name}' existe deja");
            }

            var spec = new ServiceSpec
            {
                Name = request.Name,
                Mode = new ServiceMode
                {
                    Replicated = new ReplicatedService
                    {
                        Replicas = (ulong)request.Replicas
                    }
                },
                TaskTemplate = new TaskSpec
                {
                    ContainerSpec = new ContainerSpec
                    {
                        Image = request.Image,
                        Env = request.Env?.Select(kv => $"{kv.Key}={kv.Value}").ToList()
                    }
                },
                Labels = request.Labels
            };

            // Add ports if specified
            if (request.Ports?.Any() == true)
            {
                spec.EndpointSpec = new EndpointSpec
                {
                    Ports = request.Ports.Select(p => new PortConfig
                    {
                        TargetPort = (uint)p.TargetPort,
                        PublishedPort = (uint)p.PublishedPort,
                        Protocol = p.Protocol
                    }).ToList()
                };
            }

            var createParams = new ServiceCreateParameters
            {
                Service = spec
            };

            var response = await _client.Swarm.CreateServiceAsync(createParams, cancellationToken);
            _logger.LogInformation("Service {ServiceName} created with ID {ServiceId}", request.Name, response.ID);

            return response.ID;
        }
        catch (BadRequestException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create service {ServiceName}", request.Name);
            throw new InternalServerException($"Erreur lors de la creation du service '{request.Name}'");
        }
    }

    public async Task DeleteServiceAsync(string serviceName, CancellationToken cancellationToken = default)
    {
        var service = await GetServiceByNameAsync(serviceName, cancellationToken);
        if (service == null)
        {
            throw new NotFoundException($"Service '{serviceName}' non trouve");
        }

        try
        {
            await _client.Swarm.RemoveServiceAsync(service.ID, cancellationToken);
            _logger.LogInformation("Service {ServiceName} deleted successfully", serviceName);
        }
        catch (NotFoundException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete service {ServiceName}", serviceName);
            throw new InternalServerException($"Erreur lors de la suppression du service '{serviceName}'");
        }
    }

    public async Task UpdateServiceAsync(string serviceName, UpdateServiceRequest request, CancellationToken cancellationToken = default)
    {
        var service = await GetServiceByNameAsync(serviceName, cancellationToken);
        if (service == null)
        {
            throw new NotFoundException($"Service '{serviceName}' non trouve");
        }

        try
        {
            var spec = service.Spec;

            // Update image if provided
            if (!string.IsNullOrEmpty(request.Image))
            {
                spec.TaskTemplate.ContainerSpec.Image = request.Image;
            }

            // Update replicas if provided
            if (request.Replicas.HasValue)
            {
                if (spec.Mode.Replicated == null)
                {
                    throw new BadRequestException($"Le service '{serviceName}' n'est pas en mode replicated");
                }
                spec.Mode.Replicated.Replicas = (ulong)request.Replicas.Value;
            }

            // Update environment variables if provided
            if (request.Env != null)
            {
                spec.TaskTemplate.ContainerSpec.Env = request.Env.Select(kv => $"{kv.Key}={kv.Value}").ToList();
            }

            // Update labels if provided
            if (request.Labels != null)
            {
                spec.Labels = request.Labels;
            }

            var updateParams = new ServiceUpdateParameters
            {
                Service = spec,
                Version = (long)service.Version.Index
            };

            await _client.Swarm.UpdateServiceAsync(service.ID, updateParams, cancellationToken);
            _logger.LogInformation("Service {ServiceName} updated successfully", serviceName);
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
            _logger.LogError(ex, "Failed to update service {ServiceName}", serviceName);
            throw new InternalServerException($"Erreur lors de la mise a jour du service '{serviceName}'");
        }
    }

    public async Task RollbackServiceAsync(string serviceName, CancellationToken cancellationToken = default)
    {
        var service = await GetServiceByNameAsync(serviceName, cancellationToken);
        if (service == null)
        {
            throw new NotFoundException($"Service '{serviceName}' non trouve");
        }

        try
        {
            if (service.PreviousSpec == null)
            {
                throw new BadRequestException($"Le service '{serviceName}' n'a pas de version precedente pour effectuer un rollback");
            }

            var updateParams = new ServiceUpdateParameters
            {
                Service = service.PreviousSpec,
                Version = (long)service.Version.Index
            };

            await _client.Swarm.UpdateServiceAsync(service.ID, updateParams, cancellationToken);
            _logger.LogInformation("Service {ServiceName} rolled back successfully", serviceName);
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
            _logger.LogError(ex, "Failed to rollback service {ServiceName}", serviceName);
            throw new InternalServerException($"Erreur lors du rollback du service '{serviceName}'");
        }
    }

    // Volume management methods

    public async Task<IList<VolumeResponse>> GetVolumesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var volumes = await _client.Volumes.ListAsync(cancellationToken);
            return volumes.Volumes?.ToList() ?? new List<VolumeResponse>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get Docker volumes");
            throw new InternalServerException("Docker socket non accessible");
        }
    }

    public async Task<VolumeResponse?> GetVolumeByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        try
        {
            var volume = await _client.Volumes.InspectAsync(name, cancellationToken);
            return volume;
        }
        catch (DockerApiException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get volume {VolumeName}", name);
            throw new InternalServerException($"Erreur lors de la recuperation du volume '{name}'");
        }
    }

    public async Task<IList<VolumeResponse>> GetUnusedVolumesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var volumes = await GetVolumesAsync(cancellationToken);
            var unusedVolumes = new List<VolumeResponse>();

            foreach (var volume in volumes)
            {
                var containers = await GetContainersUsingVolumeAsync(volume.Name, cancellationToken);
                if (containers.Count == 0)
                {
                    unusedVolumes.Add(volume);
                }
            }

            return unusedVolumes;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get unused volumes");
            throw new InternalServerException("Erreur lors de la recuperation des volumes inutilises");
        }
    }

    public async Task<string> CreateVolumeAsync(CreateVolumeRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            // Check if volume already exists
            var existingVolume = await GetVolumeByNameAsync(request.Name, cancellationToken);
            if (existingVolume != null)
            {
                throw new BadRequestException($"Le volume '{request.Name}' existe deja");
            }

            var createParams = new VolumesCreateParameters
            {
                Name = request.Name,
                Driver = request.Driver,
                Labels = request.Labels,
                DriverOpts = request.DriverOpts
            };

            var volume = await _client.Volumes.CreateAsync(createParams, cancellationToken);
            _logger.LogInformation("Volume {VolumeName} created successfully", request.Name);

            return volume.Name;
        }
        catch (BadRequestException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create volume {VolumeName}", request.Name);
            throw new InternalServerException($"Erreur lors de la creation du volume '{request.Name}'");
        }
    }

    public async Task DeleteVolumeAsync(string volumeName, bool force = false, CancellationToken cancellationToken = default)
    {
        var volume = await GetVolumeByNameAsync(volumeName, cancellationToken);
        if (volume == null)
        {
            throw new NotFoundException($"Volume '{volumeName}' non trouve");
        }

        try
        {
            // Check if volume is in use
            if (!force)
            {
                var containers = await GetContainersUsingVolumeAsync(volumeName, cancellationToken);
                if (containers.Count > 0)
                {
                    throw new BadRequestException($"Le volume '{volumeName}' est utilise par {containers.Count} container(s). Utilisez force=true pour forcer la suppression.");
                }
            }

            await _client.Volumes.RemoveAsync(volumeName, force, cancellationToken);
            _logger.LogInformation("Volume {VolumeName} deleted successfully", volumeName);
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
            _logger.LogError(ex, "Failed to delete volume {VolumeName}", volumeName);
            throw new InternalServerException($"Erreur lors de la suppression du volume '{volumeName}'");
        }
    }

    public async Task<(int count, long spaceReclaimed, List<string> deletedVolumes)> PruneVolumesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _client.Volumes.PruneAsync(null, cancellationToken);

            var deletedVolumes = response.VolumesDeleted?.ToList() ?? new List<string>();
            var spaceReclaimed = (long)response.SpaceReclaimed;

            _logger.LogInformation("Pruned {Count} volumes, reclaimed {SpaceReclaimed} bytes", deletedVolumes.Count, spaceReclaimed);

            return (deletedVolumes.Count, spaceReclaimed, deletedVolumes);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to prune volumes");
            throw new InternalServerException("Erreur lors du nettoyage des volumes");
        }
    }

    public async Task<BackupVolumeResponse> BackupVolumeAsync(string volumeName, string destinationPath, CancellationToken cancellationToken = default)
    {
        var volume = await GetVolumeByNameAsync(volumeName, cancellationToken);
        if (volume == null)
        {
            throw new NotFoundException($"Volume '{volumeName}' non trouve");
        }

        try
        {
            // Create a temporary container to backup the volume
            var containerParams = new CreateContainerParameters
            {
                Image = "busybox:latest",
                Cmd = new[] { "tar", "cvf", "/backup/backup.tar", "-C", "/data", "." },
                HostConfig = new HostConfig
                {
                    Binds = new[]
                    {
                        $"{volumeName}:/data:ro",
                        $"{destinationPath}:/backup"
                    },
                    AutoRemove = true
                }
            };

            // Pull busybox image if not exists
            try
            {
                await _client.Images.CreateImageAsync(
                    new ImagesCreateParameters { FromImage = "busybox", Tag = "latest" },
                    null,
                    new Progress<JSONMessage>(),
                    cancellationToken);
            }
            catch
            {
                // Image might already exist, ignore error
            }

            var container = await _client.Containers.CreateContainerAsync(containerParams, cancellationToken);

            try
            {
                await _client.Containers.StartContainerAsync(container.ID, null, cancellationToken);

                // Wait for container to finish
                await _client.Containers.WaitContainerAsync(container.ID, cancellationToken);

                var backupPath = Path.Combine(destinationPath, "backup.tar");
                var backupSize = await GetVolumeSizeAsync(volumeName, cancellationToken);

                _logger.LogInformation("Volume {VolumeName} backed up to {BackupPath}", volumeName, backupPath);

                return new BackupVolumeResponse(
                    VolumeName: volumeName,
                    BackupPath: backupPath,
                    BackupDate: DateTime.UtcNow,
                    SizeBytes: backupSize
                );
            }
            finally
            {
                // Clean up container if not auto-removed
                try
                {
                    await _client.Containers.RemoveContainerAsync(container.ID, new ContainerRemoveParameters { Force = true }, cancellationToken);
                }
                catch
                {
                    // Container might already be removed, ignore
                }
            }
        }
        catch (NotFoundException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to backup volume {VolumeName}", volumeName);
            throw new InternalServerException($"Erreur lors du backup du volume '{volumeName}'");
        }
    }

    public async Task<RestoreVolumeResponse> RestoreVolumeAsync(string volumeName, string sourcePath, CancellationToken cancellationToken = default)
    {
        try
        {
            // Create volume if it doesn't exist
            var existingVolume = await GetVolumeByNameAsync(volumeName, cancellationToken);
            if (existingVolume == null)
            {
                await CreateVolumeAsync(new CreateVolumeRequest(Name: volumeName), cancellationToken);
            }

            // Get the directory and file from source path
            var sourceDir = Path.GetDirectoryName(sourcePath) ?? sourcePath;
            var sourceFile = Path.GetFileName(sourcePath);

            // Create a temporary container to restore the volume
            var containerParams = new CreateContainerParameters
            {
                Image = "busybox:latest",
                Cmd = new[] { "tar", "xvf", $"/backup/{sourceFile}", "-C", "/data" },
                HostConfig = new HostConfig
                {
                    Binds = new[]
                    {
                        $"{volumeName}:/data",
                        $"{sourceDir}:/backup:ro"
                    },
                    AutoRemove = true
                }
            };

            // Pull busybox image if not exists
            try
            {
                await _client.Images.CreateImageAsync(
                    new ImagesCreateParameters { FromImage = "busybox", Tag = "latest" },
                    null,
                    new Progress<JSONMessage>(),
                    cancellationToken);
            }
            catch
            {
                // Image might already exist, ignore error
            }

            var container = await _client.Containers.CreateContainerAsync(containerParams, cancellationToken);

            try
            {
                await _client.Containers.StartContainerAsync(container.ID, null, cancellationToken);

                // Wait for container to finish
                await _client.Containers.WaitContainerAsync(container.ID, cancellationToken);

                _logger.LogInformation("Volume {VolumeName} restored from {SourcePath}", volumeName, sourcePath);

                return new RestoreVolumeResponse(
                    VolumeName: volumeName,
                    SourcePath: sourcePath,
                    RestoreDate: DateTime.UtcNow
                );
            }
            finally
            {
                // Clean up container if not auto-removed
                try
                {
                    await _client.Containers.RemoveContainerAsync(container.ID, new ContainerRemoveParameters { Force = true }, cancellationToken);
                }
                catch
                {
                    // Container might already be removed, ignore
                }
            }
        }
        catch (NotFoundException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to restore volume {VolumeName}", volumeName);
            throw new InternalServerException($"Erreur lors de la restauration du volume '{volumeName}'");
        }
    }

    public async Task<long> GetVolumeSizeAsync(string volumeName, CancellationToken cancellationToken = default)
    {
        var volume = await GetVolumeByNameAsync(volumeName, cancellationToken);
        if (volume == null)
        {
            throw new NotFoundException($"Volume '{volumeName}' non trouve");
        }

        try
        {
            // Use du command via a temporary container to get actual volume size
            var containerParams = new CreateContainerParameters
            {
                Image = "busybox:latest",
                Cmd = new[] { "du", "-sb", "/data" },
                HostConfig = new HostConfig
                {
                    Binds = new[] { $"{volumeName}:/data:ro" },
                    AutoRemove = true
                }
            };

            // Pull busybox image if not exists
            try
            {
                await _client.Images.CreateImageAsync(
                    new ImagesCreateParameters { FromImage = "busybox", Tag = "latest" },
                    null,
                    new Progress<JSONMessage>(),
                    cancellationToken);
            }
            catch
            {
                // Image might already exist, ignore error
            }

            var container = await _client.Containers.CreateContainerAsync(containerParams, cancellationToken);

            try
            {
                await _client.Containers.StartContainerAsync(container.ID, null, cancellationToken);

                // Wait for container to finish and get logs
                await _client.Containers.WaitContainerAsync(container.ID, cancellationToken);

                using var logsStream = await _client.Containers.GetContainerLogsAsync(
                    container.ID,
                    false,
                    new ContainerLogsParameters { ShowStdout = true, ShowStderr = false },
                    cancellationToken);

                var logs = await logsStream.ReadOutputToEndAsync(cancellationToken);
                var output = logs.stdout.Trim();

                // Parse the du output (format: "12345\t/data")
                var parts = output.Split('\t');
                if (parts.Length > 0 && long.TryParse(parts[0], out var size))
                {
                    return size;
                }

                return 0;
            }
            finally
            {
                // Clean up container if not auto-removed
                try
                {
                    await _client.Containers.RemoveContainerAsync(container.ID, new ContainerRemoveParameters { Force = true }, cancellationToken);
                }
                catch
                {
                    // Container might already be removed, ignore
                }
            }
        }
        catch (NotFoundException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get size for volume {VolumeName}", volumeName);
            // Return 0 if we can't get the size instead of throwing
            return 0;
        }
    }

    public async Task<IList<string>> GetContainersUsingVolumeAsync(string volumeName, CancellationToken cancellationToken = default)
    {
        try
        {
            var containers = await _client.Containers.ListContainersAsync(
                new ContainersListParameters { All = true },
                cancellationToken);

            var usingContainers = new List<string>();

            foreach (var container in containers)
            {
                if (container.Mounts != null)
                {
                    foreach (var mount in container.Mounts)
                    {
                        if (mount.Name == volumeName || mount.Source?.Contains(volumeName) == true)
                        {
                            var containerName = container.Names?.FirstOrDefault()?.TrimStart('/') ?? container.ID;
                            usingContainers.Add(containerName);
                            break;
                        }
                    }
                }
            }

            return usingContainers;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get containers using volume {VolumeName}", volumeName);
            return new List<string>();
        }
    }
}
