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

    // Resource management methods

    public async Task UpdateServiceResourcesAsync(
        string serviceName,
        long? cpuLimitNanoCpus,
        long? cpuReservationNanoCpus,
        long? memoryLimitBytes,
        long? memoryReservationBytes,
        long? pidsLimit,
        List<UlimitDTO>? ulimits,
        CancellationToken cancellationToken = default)
    {
        var service = await GetServiceByNameAsync(serviceName, cancellationToken);
        if (service == null)
        {
            throw new NotFoundException($"Service '{serviceName}' non trouve");
        }

        try
        {
            var spec = service.Spec;

            // Initialiser les resources si necessaire
            if (spec.TaskTemplate.Resources == null)
            {
                spec.TaskTemplate.Resources = new ResourceRequirements();
            }
            if (spec.TaskTemplate.Resources.Limits == null)
            {
                spec.TaskTemplate.Resources.Limits = new SwarmLimit();
            }
            if (spec.TaskTemplate.Resources.Reservations == null)
            {
                spec.TaskTemplate.Resources.Reservations = new SwarmResources();
            }

            // Appliquer les limites CPU
            if (cpuLimitNanoCpus.HasValue)
            {
                spec.TaskTemplate.Resources.Limits.NanoCPUs = cpuLimitNanoCpus.Value;
            }

            // Appliquer la reservation CPU
            if (cpuReservationNanoCpus.HasValue)
            {
                spec.TaskTemplate.Resources.Reservations.NanoCPUs = cpuReservationNanoCpus.Value;
            }

            // Appliquer les limites memoire
            if (memoryLimitBytes.HasValue)
            {
                spec.TaskTemplate.Resources.Limits.MemoryBytes = memoryLimitBytes.Value;
            }

            // Appliquer la reservation memoire
            if (memoryReservationBytes.HasValue)
            {
                spec.TaskTemplate.Resources.Reservations.MemoryBytes = memoryReservationBytes.Value;
            }

            // Appliquer la limite de PIDs
            if (pidsLimit.HasValue)
            {
                spec.TaskTemplate.Resources.Limits.Pids = pidsLimit.Value;
            }

            // Appliquer les ulimits
            if (ulimits != null && ulimits.Count > 0)
            {
                spec.TaskTemplate.ContainerSpec.Ulimits = ulimits.Select(u => new Ulimit
                {
                    Name = u.Name,
                    Soft = u.Soft,
                    Hard = u.Hard
                }).ToList();
            }

            var updateParams = new ServiceUpdateParameters
            {
                Service = spec,
                Version = (long)service.Version.Index
            };

            await _client.Swarm.UpdateServiceAsync(service.ID, updateParams, cancellationToken);
            _logger.LogInformation(
                "Service {ServiceName} resources updated: CPU limit={CpuLimit}, Memory limit={MemLimit}",
                serviceName,
                cpuLimitNanoCpus,
                memoryLimitBytes);
        }
        catch (NotFoundException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to update resources for service {ServiceName}", serviceName);
            throw new InternalServerException($"Erreur lors de la mise a jour des ressources du service '{serviceName}'");
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
                    // AutoRemove = false pour éviter une condition de course (race condition)
                    // entre la suppression automatique du conteneur et la récupération des logs
                    // Le nettoyage manuel est effectué dans le bloc finally
                    AutoRemove = false
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

    // Network management methods

    public async Task<IList<NetworkResponse>> GetNetworksAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var networks = await _client.Networks.ListNetworksAsync(null, cancellationToken);
            return networks.ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get Docker networks");
            throw new InternalServerException("Docker socket non accessible");
        }
    }

    public async Task<NetworkResponse?> GetNetworkByNameAsync(string name, CancellationToken cancellationToken = default)
    {
        try
        {
            var networks = await GetNetworksAsync(cancellationToken);
            return networks.FirstOrDefault(n => n.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get network {NetworkName}", name);
            throw new InternalServerException($"Erreur lors de la recuperation du reseau '{name}'");
        }
    }

    public async Task<string> CreateNetworkAsync(CreateNetworkRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            // Check if network already exists
            var existingNetwork = await GetNetworkByNameAsync(request.Name, cancellationToken);
            if (existingNetwork != null)
            {
                throw new BadRequestException($"Le reseau '{request.Name}' existe deja");
            }

            var createParams = new NetworksCreateParameters
            {
                Name = request.Name,
                Driver = request.Driver,
                Attachable = request.IsAttachable,
                Labels = request.Labels,
                Options = request.Options
            };

            // Add IPAM configuration if subnet is specified
            if (!string.IsNullOrEmpty(request.Subnet))
            {
                createParams.IPAM = new IPAM
                {
                    Config = new List<IPAMConfig>
                    {
                        new IPAMConfig
                        {
                            Subnet = request.Subnet,
                            Gateway = request.Gateway,
                            IPRange = request.IpRange
                        }
                    }
                };
            }

            var response = await _client.Networks.CreateNetworkAsync(createParams, cancellationToken);
            _logger.LogInformation("Network {NetworkName} created with ID {NetworkId}", request.Name, response.ID);

            return response.ID;
        }
        catch (BadRequestException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to create network {NetworkName}", request.Name);
            throw new InternalServerException($"Erreur lors de la creation du reseau '{request.Name}'");
        }
    }

    public async Task DeleteNetworkAsync(string networkName, CancellationToken cancellationToken = default)
    {
        var network = await GetNetworkByNameAsync(networkName, cancellationToken);
        if (network == null)
        {
            throw new NotFoundException($"Reseau '{networkName}' non trouve");
        }

        try
        {
            await _client.Networks.DeleteNetworkAsync(network.ID, cancellationToken);
            _logger.LogInformation("Network {NetworkName} deleted successfully", networkName);
        }
        catch (NotFoundException)
        {
            throw;
        }
        catch (DockerApiException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Forbidden)
        {
            throw new BadRequestException($"Le reseau '{networkName}' ne peut pas etre supprime (reseau systeme ou en cours d'utilisation)");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete network {NetworkName}", networkName);
            throw new InternalServerException($"Erreur lors de la suppression du reseau '{networkName}'");
        }
    }

    public async Task<(int count, List<string> deletedNetworks)> PruneNetworksAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _client.Networks.PruneNetworksAsync(null, cancellationToken);

            var deletedNetworks = response.NetworksDeleted?.ToList() ?? new List<string>();

            _logger.LogInformation("Pruned {Count} networks", deletedNetworks.Count);

            return (deletedNetworks.Count, deletedNetworks);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to prune networks");
            throw new InternalServerException("Erreur lors du nettoyage des reseaux");
        }
    }

    public async Task ConnectContainerToNetworkAsync(string networkName, ConnectContainerRequest request, CancellationToken cancellationToken = default)
    {
        var network = await GetNetworkByNameAsync(networkName, cancellationToken);
        if (network == null)
        {
            throw new NotFoundException($"Reseau '{networkName}' non trouve");
        }

        var container = await GetContainerByIdAsync(request.ContainerId, cancellationToken);
        if (container == null)
        {
            throw new NotFoundException($"Conteneur '{request.ContainerId}' non trouve");
        }

        try
        {
            var connectParams = new NetworkConnectParameters
            {
                Container = request.ContainerId
            };

            // Add IP address if specified
            if (!string.IsNullOrEmpty(request.IpAddress))
            {
                connectParams.EndpointConfig = new EndpointSettings
                {
                    IPAMConfig = new EndpointIPAMConfig
                    {
                        IPv4Address = request.IpAddress
                    }
                };
            }

            await _client.Networks.ConnectNetworkAsync(network.ID, connectParams, cancellationToken);
            _logger.LogInformation("Container {ContainerId} connected to network {NetworkName}", request.ContainerId, networkName);
        }
        catch (NotFoundException)
        {
            throw;
        }
        catch (DockerApiException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Forbidden)
        {
            throw new BadRequestException($"Impossible de connecter le conteneur au reseau '{networkName}'");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to connect container {ContainerId} to network {NetworkName}", request.ContainerId, networkName);
            throw new InternalServerException($"Erreur lors de la connexion du conteneur au reseau '{networkName}'");
        }
    }

    public async Task DisconnectContainerFromNetworkAsync(string networkName, DisconnectContainerRequest request, CancellationToken cancellationToken = default)
    {
        var network = await GetNetworkByNameAsync(networkName, cancellationToken);
        if (network == null)
        {
            throw new NotFoundException($"Reseau '{networkName}' non trouve");
        }

        var container = await GetContainerByIdAsync(request.ContainerId, cancellationToken);
        if (container == null)
        {
            throw new NotFoundException($"Conteneur '{request.ContainerId}' non trouve");
        }

        try
        {
            var disconnectParams = new NetworkDisconnectParameters
            {
                Container = request.ContainerId,
                Force = request.Force
            };

            await _client.Networks.DisconnectNetworkAsync(network.ID, disconnectParams, cancellationToken);
            _logger.LogInformation("Container {ContainerId} disconnected from network {NetworkName}", request.ContainerId, networkName);
        }
        catch (NotFoundException)
        {
            throw;
        }
        catch (DockerApiException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Forbidden)
        {
            throw new BadRequestException($"Impossible de deconnecter le conteneur du reseau '{networkName}'");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to disconnect container {ContainerId} from network {NetworkName}", request.ContainerId, networkName);
            throw new InternalServerException($"Erreur lors de la deconnexion du conteneur du reseau '{networkName}'");
        }
    }

    // Image management methods

    public async Task<IList<ImagesListResponse>> GetImagesAsync(bool all = false, CancellationToken cancellationToken = default)
    {
        try
        {
            var images = await _client.Images.ListImagesAsync(
                new ImagesListParameters { All = all },
                cancellationToken);
            return images.ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get Docker images");
            throw new InternalServerException("Docker socket non accessible");
        }
    }

    public async Task<ImageInspectResponse?> GetImageByIdAsync(string imageId, CancellationToken cancellationToken = default)
    {
        try
        {
            var image = await _client.Images.InspectImageAsync(imageId, cancellationToken);
            return image;
        }
        catch (DockerApiException ex) when (ex.StatusCode == System.Net.HttpStatusCode.NotFound)
        {
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get image {ImageId}", imageId);
            throw new InternalServerException($"Erreur lors de la recuperation de l'image '{imageId}'");
        }
    }

    public async Task<IList<ImageHistoryResponse>> GetImageHistoryAsync(string imageId, CancellationToken cancellationToken = default)
    {
        var image = await GetImageByIdAsync(imageId, cancellationToken);
        if (image == null)
        {
            throw new NotFoundException($"Image '{imageId}' non trouvee");
        }

        try
        {
            var history = await _client.Images.GetImageHistoryAsync(imageId, cancellationToken);
            return history.ToList();
        }
        catch (NotFoundException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get history for image {ImageId}", imageId);
            throw new InternalServerException($"Erreur lors de la recuperation de l'historique de l'image '{imageId}'");
        }
    }

    public async Task<IList<ImagesListResponse>> GetDanglingImagesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var images = await _client.Images.ListImagesAsync(
                new ImagesListParameters
                {
                    Filters = new Dictionary<string, IDictionary<string, bool>>
                    {
                        ["dangling"] = new Dictionary<string, bool> { ["true"] = true }
                    }
                },
                cancellationToken);
            return images.ToList();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get dangling images");
            throw new InternalServerException("Erreur lors de la recuperation des images dangling");
        }
    }

    public async Task<PullImageResponse> PullImageAsync(PullImageRequest request, CancellationToken cancellationToken = default)
    {
        try
        {
            var imageName = request.Image;
            var tag = request.Tag ?? "latest";

            // Add registry prefix if specified
            if (!string.IsNullOrEmpty(request.Registry))
            {
                imageName = $"{request.Registry}/{imageName}";
            }

            var createParams = new ImagesCreateParameters
            {
                FromImage = imageName,
                Tag = tag
            };

            // Use progress handler to track pull progress
            var lastStatus = "";
            await _client.Images.CreateImageAsync(
                createParams,
                null, // authConfig - could be extended to support private registries
                new Progress<JSONMessage>(message =>
                {
                    if (!string.IsNullOrEmpty(message.Status))
                    {
                        lastStatus = message.Status;
                    }
                }),
                cancellationToken);

            _logger.LogInformation("Image {ImageName}:{Tag} pulled successfully", imageName, tag);

            return new PullImageResponse(
                ImageName: imageName,
                Tag: tag,
                Status: lastStatus ?? "Pull completed",
                PulledAt: DateTime.UtcNow
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to pull image {ImageName}", request.Image);
            throw new InternalServerException($"Erreur lors du pull de l'image '{request.Image}:{request.Tag ?? "latest"}'");
        }
    }

    public async Task DeleteImageAsync(string imageId, bool force = false, bool pruneChildren = false, CancellationToken cancellationToken = default)
    {
        var image = await GetImageByIdAsync(imageId, cancellationToken);
        if (image == null)
        {
            throw new NotFoundException($"Image '{imageId}' non trouvee");
        }

        try
        {
            var deleteParams = new ImageDeleteParameters
            {
                Force = force,
                NoPrune = !pruneChildren
            };

            await _client.Images.DeleteImageAsync(imageId, deleteParams, cancellationToken);
            _logger.LogInformation("Image {ImageId} deleted successfully", imageId);
        }
        catch (NotFoundException)
        {
            throw;
        }
        catch (DockerApiException ex) when (ex.StatusCode == System.Net.HttpStatusCode.Conflict)
        {
            throw new BadRequestException($"L'image '{imageId}' est utilisee par un ou plusieurs conteneurs. Utilisez force=true pour forcer la suppression.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete image {ImageId}", imageId);
            throw new InternalServerException($"Erreur lors de la suppression de l'image '{imageId}'");
        }
    }

    public async Task<TagImageResponse> TagImageAsync(string imageId, TagImageRequest request, CancellationToken cancellationToken = default)
    {
        var image = await GetImageByIdAsync(imageId, cancellationToken);
        if (image == null)
        {
            throw new NotFoundException($"Image '{imageId}' non trouvee");
        }

        try
        {
            var tagParams = new ImageTagParameters
            {
                RepositoryName = request.NewRepository,
                Tag = request.NewTag
            };

            await _client.Images.TagImageAsync(imageId, tagParams, cancellationToken);
            _logger.LogInformation("Image {ImageId} tagged as {Repository}:{Tag}", imageId, request.NewRepository, request.NewTag);

            return new TagImageResponse(
                SourceImage: imageId,
                NewRepository: request.NewRepository,
                NewTag: request.NewTag
            );
        }
        catch (NotFoundException)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to tag image {ImageId}", imageId);
            throw new InternalServerException($"Erreur lors du tagging de l'image '{imageId}'");
        }
    }

    public async Task<PushImageResponse> PushImageAsync(string imageId, PushImageRequest request, CancellationToken cancellationToken = default)
    {
        var image = await GetImageByIdAsync(imageId, cancellationToken);
        if (image == null)
        {
            throw new NotFoundException($"Image '{imageId}' non trouvee");
        }

        try
        {
            // Get the first repo tag to push
            var repoTag = image.RepoTags?.FirstOrDefault();
            if (string.IsNullOrEmpty(repoTag))
            {
                throw new BadRequestException($"L'image '{imageId}' n'a pas de tag. Veuillez d'abord la taguer.");
            }

            var parts = repoTag.Split(':');
            var repository = parts[0];
            var tag = request.Tag ?? (parts.Length > 1 ? parts[1] : "latest");

            // Add registry prefix if specified
            if (!string.IsNullOrEmpty(request.Registry))
            {
                repository = $"{request.Registry}/{repository}";
            }

            var lastStatus = "";
            await _client.Images.PushImageAsync(
                $"{repository}:{tag}",
                new ImagePushParameters(),
                null, // authConfig - could be extended to support private registries
                new Progress<JSONMessage>(message =>
                {
                    if (!string.IsNullOrEmpty(message.Status))
                    {
                        lastStatus = message.Status;
                    }
                }),
                cancellationToken);

            _logger.LogInformation("Image {Repository}:{Tag} pushed successfully", repository, tag);

            return new PushImageResponse(
                ImageName: repository,
                Tag: tag,
                Status: lastStatus ?? "Push completed",
                PushedAt: DateTime.UtcNow
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
            _logger.LogError(ex, "Failed to push image {ImageId}", imageId);
            throw new InternalServerException($"Erreur lors du push de l'image '{imageId}'");
        }
    }

    public async Task<(int count, long spaceReclaimed, List<string> deletedImages)> PruneImagesAsync(bool dangling = true, CancellationToken cancellationToken = default)
    {
        try
        {
            var filters = new Dictionary<string, IDictionary<string, bool>>();
            if (dangling)
            {
                filters["dangling"] = new Dictionary<string, bool> { ["true"] = true };
            }

            var response = await _client.Images.PruneImagesAsync(
                new ImagesPruneParameters { Filters = filters },
                cancellationToken);

            var deletedImages = response.ImagesDeleted?
                .Where(i => !string.IsNullOrEmpty(i.Deleted))
                .Select(i => i.Deleted!)
                .ToList() ?? new List<string>();

            var spaceReclaimed = (long)response.SpaceReclaimed;

            _logger.LogInformation("Pruned {Count} images, reclaimed {SpaceReclaimed} bytes", deletedImages.Count, spaceReclaimed);

            return (deletedImages.Count, spaceReclaimed, deletedImages);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to prune images");
            throw new InternalServerException("Erreur lors du nettoyage des images");
        }
    }

    public async Task<int> GetImageContainerCountAsync(string imageId, CancellationToken cancellationToken = default)
    {
        try
        {
            var containers = await _client.Containers.ListContainersAsync(
                new ContainersListParameters { All = true },
                cancellationToken);

            // Count containers using this image (by ID or name)
            var count = containers.Count(c =>
                c.ImageID == imageId ||
                c.ImageID == $"sha256:{imageId}" ||
                c.Image == imageId);

            return count;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get container count for image {ImageId}", imageId);
            return 0;
        }
    }

    // Stack management methods
    // Docker Swarm stacks are identified by the "com.docker.stack.namespace" label on services

    public async Task<IList<StackDTO>> GetStacksAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var services = await GetServicesAsync(cancellationToken);

            // Group services by stack name (from label com.docker.stack.namespace)
            var stacks = services
                .Where(s => s.Spec?.Labels?.ContainsKey("com.docker.stack.namespace") == true)
                .GroupBy(s => s.Spec.Labels["com.docker.stack.namespace"])
                .Select(g => new StackDTO(
                    Name: g.Key,
                    ServiceCount: g.Count(),
                    Orchestrator: "swarm",
                    CreatedAt: g.Min(s => s.CreatedAt)
                ))
                .OrderBy(s => s.Name)
                .ToList();

            return stacks;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get Docker stacks");
            throw new InternalServerException("Erreur lors de la recuperation des stacks Docker");
        }
    }

    public async Task<StackDetailsDTO?> GetStackByNameAsync(string stackName, CancellationToken cancellationToken = default)
    {
        try
        {
            var services = await GetServicesAsync(cancellationToken);

            // Filter services belonging to the stack
            var stackServices = services
                .Where(s => s.Spec?.Labels?.ContainsKey("com.docker.stack.namespace") == true
                         && s.Spec.Labels["com.docker.stack.namespace"] == stackName)
                .ToList();

            if (stackServices.Count == 0)
            {
                return null;
            }

            var serviceDtos = stackServices.Select(s =>
            {
                var ports = s.Endpoint?.Ports?
                    .Select(p => new StackServicePortDTO(
                        TargetPort: (int)p.TargetPort,
                        PublishedPort: (int)p.PublishedPort,
                        Protocol: p.Protocol ?? "tcp"
                    ))
                    .ToList() ?? new List<StackServicePortDTO>();

                var runningReplicas = 0;
                var desiredReplicas = (int)(s.Spec?.Mode?.Replicated?.Replicas ?? 1);

                // For mode global, count nodes
                if (s.Spec?.Mode?.Global != null)
                {
                    desiredReplicas = 1; // Global services have 1 per node
                }

                // Check running replicas from ServiceStatus if available
                if (s.ServiceStatus != null)
                {
                    runningReplicas = (int)s.ServiceStatus.RunningTasks;
                    desiredReplicas = (int)s.ServiceStatus.DesiredTasks;
                }

                var status = runningReplicas >= desiredReplicas ? "running" : "updating";

                return new StackServiceDTO(
                    Id: s.ID,
                    Name: s.Spec?.Name ?? "",
                    Image: s.Spec?.TaskTemplate?.ContainerSpec?.Image?.Split('@')[0] ?? "",
                    Replicas: runningReplicas,
                    DesiredReplicas: desiredReplicas,
                    Status: status,
                    Ports: ports
                );
            }).ToList();

            return new StackDetailsDTO(
                Name: stackName,
                ServiceCount: stackServices.Count,
                Orchestrator: "swarm",
                CreatedAt: stackServices.Min(s => s.CreatedAt),
                Services: serviceDtos
            );
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to get stack {StackName}", stackName);
            throw new InternalServerException($"Erreur lors de la recuperation de la stack '{stackName}'");
        }
    }

    public async Task<IList<StackServiceDTO>> GetStackServicesAsync(string stackName, CancellationToken cancellationToken = default)
    {
        var stackDetails = await GetStackByNameAsync(stackName, cancellationToken);
        if (stackDetails == null)
        {
            throw new NotFoundException($"Stack '{stackName}' non trouvee");
        }
        return stackDetails.Services;
    }

    public async Task<DeployStackResponse> DeployStackAsync(DeployStackRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            throw new BadRequestException("Le nom de la stack est requis");
        }

        if (string.IsNullOrWhiteSpace(request.ComposeFileContent))
        {
            throw new BadRequestException("Le contenu du fichier compose est requis");
        }

        try
        {
            // Parse the compose file to extract services
            var composeContent = request.ComposeFileContent;

            // Create a temporary file for the compose content
            var tempDir = Path.Combine(Path.GetTempPath(), "docker-stacks", request.Name);
            Directory.CreateDirectory(tempDir);
            var composeFilePath = Path.Combine(tempDir, "docker-compose.yml");

            await File.WriteAllTextAsync(composeFilePath, composeContent, cancellationToken);

            try
            {
                // Use docker stack deploy command via process
                var args = $"stack deploy -c \"{composeFilePath}\" {request.Name}";
                if (request.Prune)
                {
                    args += " --prune";
                }

                var processStartInfo = new System.Diagnostics.ProcessStartInfo
                {
                    FileName = "docker",
                    Arguments = args,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using var process = System.Diagnostics.Process.Start(processStartInfo);
                if (process == null)
                {
                    throw new InternalServerException("Impossible de demarrer le processus docker");
                }

                var output = await process.StandardOutput.ReadToEndAsync(cancellationToken);
                var error = await process.StandardError.ReadToEndAsync(cancellationToken);

                await process.WaitForExitAsync(cancellationToken);

                if (process.ExitCode != 0)
                {
                    _logger.LogError("Docker stack deploy failed: {Error}", error);
                    throw new BadRequestException($"Echec du deploiement de la stack: {error}");
                }

                _logger.LogInformation("Stack {StackName} deployed successfully", request.Name);

                // Get the deployed stack to count services
                var deployedStack = await GetStackByNameAsync(request.Name, cancellationToken);

                return new DeployStackResponse(
                    StackName: request.Name,
                    ServicesDeployed: deployedStack?.ServiceCount ?? 0,
                    DeployedAt: DateTime.UtcNow
                );
            }
            finally
            {
                // Cleanup temp file
                try
                {
                    if (File.Exists(composeFilePath))
                    {
                        File.Delete(composeFilePath);
                    }
                    if (Directory.Exists(tempDir))
                    {
                        Directory.Delete(tempDir, true);
                    }
                }
                catch
                {
                    // Ignore cleanup errors
                }
            }
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
            _logger.LogError(ex, "Failed to deploy stack {StackName}", request.Name);
            throw new InternalServerException($"Erreur lors du deploiement de la stack '{request.Name}'");
        }
    }

    public async Task DeleteStackAsync(string stackName, CancellationToken cancellationToken = default)
    {
        // Verify stack exists
        var stack = await GetStackByNameAsync(stackName, cancellationToken);
        if (stack == null)
        {
            throw new NotFoundException($"Stack '{stackName}' non trouvee");
        }

        try
        {
            // Use docker stack rm command via process
            var processStartInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = "docker",
                Arguments = $"stack rm {stackName}",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            using var process = System.Diagnostics.Process.Start(processStartInfo);
            if (process == null)
            {
                throw new InternalServerException("Impossible de demarrer le processus docker");
            }

            var output = await process.StandardOutput.ReadToEndAsync(cancellationToken);
            var error = await process.StandardError.ReadToEndAsync(cancellationToken);

            await process.WaitForExitAsync(cancellationToken);

            if (process.ExitCode != 0)
            {
                _logger.LogError("Docker stack rm failed: {Error}", error);
                throw new BadRequestException($"Echec de la suppression de la stack: {error}");
            }

            _logger.LogInformation("Stack {StackName} deleted successfully", stackName);
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
            _logger.LogError(ex, "Failed to delete stack {StackName}", stackName);
            throw new InternalServerException($"Erreur lors de la suppression de la stack '{stackName}'");
        }
    }
}
