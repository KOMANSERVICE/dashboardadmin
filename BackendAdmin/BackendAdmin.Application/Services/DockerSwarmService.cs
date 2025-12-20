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

    // Container management methods

    public async Task<IList<ContainerListResponse>> GetContainersAsync(bool all = true, CancellationToken cancellationToken = default)
    {
        try
        {
            var containers = await _client.Containers.ListContainersAsync(
                new ContainersListParameters { All = all },
                cancellationToken);
            return containers.ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get Docker containers");
            throw new InternalServerException("Docker socket non accessible");
        }
    }

    public async Task<ContainerInspectResponse?> GetContainerByIdAsync(string containerId, CancellationToken cancellationToken = default)
    {
        try
        {
            var container = await _client.Containers.InspectContainerAsync(containerId, cancellationToken);
            return container;
        }
        catch (DockerApiException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get container {ContainerId}", containerId);
            throw new InternalServerException($"Erreur lors de la recuperation du conteneur '{containerId}'");
        }
    }

    public async Task<ContainerStatsDTO> GetContainerStatsAsync(string containerId, CancellationToken cancellationToken = default)
    {
        var container = await GetContainerByIdAsync(containerId, cancellationToken);
        if (container == null)
        {
            throw new NotFoundException($"Conteneur '{containerId}' non trouve");
        }

        try
        {
            // Get one-shot stats (not streaming)
            var statsParams = new ContainerStatsParameters { Stream = false };

            using var statsStream = await _client.Containers.GetContainerStatsAsync(containerId, statsParams, cancellationToken);

            // Read the stats response
            using var reader = new StreamReader(statsStream);
            var statsJson = await reader.ReadToEndAsync(cancellationToken);

            var stats = System.Text.Json.JsonSerializer.Deserialize<ContainerStatsResponse>(statsJson,
                new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (stats == null)
            {
                throw new InternalServerException($"Impossible de parser les stats du conteneur '{containerId}'");
            }

            // Calculate CPU percentage
            var cpuDelta = (double)(stats.CPUStats?.CPUUsage?.TotalUsage ?? 0) - (double)(stats.PreCPUStats?.CPUUsage?.TotalUsage ?? 0);
            var systemDelta = (double)(stats.CPUStats?.SystemUsage ?? 0) - (double)(stats.PreCPUStats?.SystemUsage ?? 0);
            var onlineCpus = stats.CPUStats?.OnlineCPUs ?? 1;
            var cpuPercent = systemDelta > 0 ? (cpuDelta / systemDelta) * onlineCpus * 100.0 : 0;

            // Calculate memory percentage
            var memoryUsage = stats.MemoryStats?.Usage ?? 0;
            var memoryLimit = stats.MemoryStats?.Limit ?? 1;
            var memoryPercent = (double)memoryUsage / memoryLimit * 100.0;

            // Calculate network stats
            ulong rxBytes = 0, txBytes = 0, rxPackets = 0, txPackets = 0;
            if (stats.Networks != null)
            {
                foreach (var network in stats.Networks.Values)
                {
                    rxBytes += network.RxBytes;
                    txBytes += network.TxBytes;
                    rxPackets += network.RxPackets;
                    txPackets += network.TxPackets;
                }
            }

            // Calculate block I/O stats
            ulong readBytes = 0, writeBytes = 0;
            if (stats.BlkioStats?.IoServiceBytesRecursive != null)
            {
                foreach (var io in stats.BlkioStats.IoServiceBytesRecursive)
                {
                    if (string.Equals(io.Op, "Read", StringComparison.OrdinalIgnoreCase))
                        readBytes += io.Value;
                    else if (string.Equals(io.Op, "Write", StringComparison.OrdinalIgnoreCase))
                        writeBytes += io.Value;
                }
            }

            var containerName = container.Name?.TrimStart('/') ?? containerId;

            return new ContainerStatsDTO(
                ContainerId: containerId,
                ContainerName: containerName,
                ReadAt: DateTime.UtcNow,
                Cpu: new ContainerCpuStatsDTO(
                    UsagePercent: cpuPercent,
                    TotalUsage: stats.CPUStats?.CPUUsage?.TotalUsage ?? 0,
                    SystemUsage: stats.CPUStats?.SystemUsage ?? 0,
                    OnlineCpus: (int)(stats.CPUStats?.OnlineCPUs ?? 1)
                ),
                Memory: new ContainerMemoryStatsDTO(
                    Usage: memoryUsage,
                    MaxUsage: stats.MemoryStats?.MaxUsage ?? 0,
                    Limit: memoryLimit,
                    UsagePercent: memoryPercent
                ),
                Network: new ContainerNetworkStatsDTO(
                    RxBytes: rxBytes,
                    TxBytes: txBytes,
                    RxPackets: rxPackets,
                    TxPackets: txPackets
                ),
                BlockIO: new ContainerBlockIOStatsDTO(
                    ReadBytes: readBytes,
                    WriteBytes: writeBytes
                )
            );
        }
        catch (NotFoundException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get stats for container {ContainerId}", containerId);
            throw new InternalServerException($"Erreur lors de la recuperation des stats du conteneur '{containerId}'");
        }
    }

    public async Task<ContainerSizeDTO> GetContainerSizeAsync(string containerId, CancellationToken cancellationToken = default)
    {
        var container = await GetContainerByIdAsync(containerId, cancellationToken);
        if (container == null)
        {
            throw new NotFoundException($"Conteneur '{containerId}' non trouve");
        }

        try
        {
            // The inspect response with size=true contains size info
            // We need to use a different approach - list containers with size option
            var containers = await _client.Containers.ListContainersAsync(
                new ContainersListParameters
                {
                    All = true,
                    Size = true,
                    Filters = new Dictionary<string, IDictionary<string, bool>>
                    {
                        ["id"] = new Dictionary<string, bool> { [containerId] = true }
                    }
                },
                cancellationToken);

            var containerWithSize = containers.FirstOrDefault();
            var containerName = container.Name?.TrimStart('/') ?? containerId;

            return new ContainerSizeDTO(
                ContainerId: containerId,
                ContainerName: containerName,
                SizeRootFs: containerWithSize?.SizeRootFs ?? 0,
                SizeRw: containerWithSize?.SizeRw ?? 0
            );
        }
        catch (NotFoundException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get size for container {ContainerId}", containerId);
            throw new InternalServerException($"Erreur lors de la recuperation de la taille du conteneur '{containerId}'");
        }
    }

    public async Task<string> GetContainerLogsAsync(string containerId, int? tail = null, bool timestamps = false, CancellationToken cancellationToken = default)
    {
        var container = await GetContainerByIdAsync(containerId, cancellationToken);
        if (container == null)
        {
            throw new NotFoundException($"Conteneur '{containerId}' non trouve");
        }

        try
        {
            var parameters = new ContainerLogsParameters
            {
                ShowStdout = true,
                ShowStderr = true,
                Timestamps = timestamps
            };

            if (tail.HasValue)
            {
                parameters.Tail = tail.Value.ToString();
            }

            using var logsStream = await _client.Containers.GetContainerLogsAsync(containerId, false, parameters, cancellationToken);
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
            _logger.LogError(ex, "Failed to get logs for container {ContainerId}", containerId);
            throw new InternalServerException($"Erreur lors de la recuperation des logs du conteneur '{containerId}'");
        }
    }

    public async Task<ContainerExecResponse> ExecContainerAsync(string containerId, ContainerExecRequest request, CancellationToken cancellationToken = default)
    {
        var container = await GetContainerByIdAsync(containerId, cancellationToken);
        if (container == null)
        {
            throw new NotFoundException($"Conteneur '{containerId}' non trouve");
        }

        // Check if container is running
        if (container.State?.Running != true)
        {
            throw new BadRequestException($"Le conteneur '{containerId}' n'est pas en cours d'execution");
        }

        try
        {
            // Build command array
            var cmd = new List<string>();

            // Parse the command string - handle both simple commands and complex ones
            if (!string.IsNullOrEmpty(request.Command))
            {
                // Use shell to handle complex commands
                cmd.Add("/bin/sh");
                cmd.Add("-c");

                var fullCommand = request.Command;
                if (request.Args?.Any() == true)
                {
                    fullCommand += " " + string.Join(" ", request.Args);
                }
                cmd.Add(fullCommand);
            }

            var execCreateParams = new ContainerExecCreateParameters
            {
                AttachStdout = request.AttachStdout,
                AttachStderr = request.AttachStderr,
                Cmd = cmd,
                WorkingDir = request.WorkingDir,
                Env = request.Env?.Select(kv => $"{kv.Key}={kv.Value}").ToList()
            };

            var execCreateResponse = await _client.Exec.ExecCreateContainerAsync(containerId, execCreateParams, cancellationToken);

            // Start the exec instance
            using var stream = await _client.Exec.StartAndAttachContainerExecAsync(
                execCreateResponse.ID,
                true,
                cancellationToken);

            // Read output
            var output = await stream.ReadOutputToEndAsync(cancellationToken);

            // Get exec exit code
            var execInspect = await _client.Exec.InspectContainerExecAsync(execCreateResponse.ID, cancellationToken);

            var containerName = container.Name?.TrimStart('/') ?? containerId;

            return new ContainerExecResponse(
                ContainerId: containerId,
                ContainerName: containerName,
                Command: request.Command,
                ExitCode: (int)execInspect.ExitCode,
                Stdout: output.stdout,
                Stderr: output.stderr,
                ExecutedAt: DateTime.UtcNow
            );
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
            _logger.LogError(ex, "Failed to exec in container {ContainerId}", containerId);
            throw new InternalServerException($"Erreur lors de l'execution de la commande dans le conteneur '{containerId}'");
        }
    }

    public async Task<ContainerTopDTO> GetContainerTopAsync(string containerId, CancellationToken cancellationToken = default)
    {
        var container = await GetContainerByIdAsync(containerId, cancellationToken);
        if (container == null)
        {
            throw new NotFoundException($"Conteneur '{containerId}' non trouve");
        }

        // Check if container is running
        if (container.State?.Running != true)
        {
            throw new BadRequestException($"Le conteneur '{containerId}' n'est pas en cours d'execution");
        }

        try
        {
            var topResponse = await _client.Containers.ListProcessesAsync(containerId, new ContainerListProcessesParameters(), cancellationToken);

            var containerName = container.Name?.TrimStart('/') ?? containerId;
            var titles = topResponse.Titles?.ToList() ?? new List<string>();

            var processes = new List<ContainerProcessDTO>();

            if (topResponse.Processes != null)
            {
                foreach (var process in topResponse.Processes)
                {
                    // Map process columns to DTO properties based on title order
                    var processDict = new Dictionary<string, string>();
                    for (int i = 0; i < titles.Count && i < process.Count; i++)
                    {
                        processDict[titles[i].ToUpperInvariant()] = process[i];
                    }

                    processes.Add(new ContainerProcessDTO(
                        Pid: processDict.GetValueOrDefault("PID", ""),
                        User: processDict.GetValueOrDefault("USER", ""),
                        Cpu: processDict.GetValueOrDefault("%CPU", processDict.GetValueOrDefault("CPU", "")),
                        Memory: processDict.GetValueOrDefault("%MEM", processDict.GetValueOrDefault("MEM", "")),
                        Vsz: processDict.GetValueOrDefault("VSZ", ""),
                        Rss: processDict.GetValueOrDefault("RSS", ""),
                        Tty: processDict.GetValueOrDefault("TTY", ""),
                        Stat: processDict.GetValueOrDefault("STAT", ""),
                        Start: processDict.GetValueOrDefault("START", ""),
                        Time: processDict.GetValueOrDefault("TIME", ""),
                        Command: processDict.GetValueOrDefault("COMMAND", processDict.GetValueOrDefault("CMD", ""))
                    ));
                }
            }

            return new ContainerTopDTO(
                ContainerId: containerId,
                ContainerName: containerName,
                Titles: titles,
                Processes: processes
            );
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
            _logger.LogError(ex, "Failed to get processes for container {ContainerId}", containerId);
            throw new InternalServerException($"Erreur lors de la recuperation des processus du conteneur '{containerId}'");
        }
    }

    public async Task<IList<ContainerFileSystemChangeResponse>> GetContainerChangesAsync(string containerId, CancellationToken cancellationToken = default)
    {
        var container = await GetContainerByIdAsync(containerId, cancellationToken);
        if (container == null)
        {
            throw new NotFoundException($"Conteneur '{containerId}' non trouve");
        }

        try
        {
            var changes = await _client.Containers.InspectChangesAsync(containerId, cancellationToken);
            return changes?.ToList() ?? new List<ContainerFileSystemChangeResponse>();
        }
        catch (NotFoundException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get filesystem changes for container {ContainerId}", containerId);
            throw new InternalServerException($"Erreur lors de la recuperation des modifications du conteneur '{containerId}'");
        }
    }
}
