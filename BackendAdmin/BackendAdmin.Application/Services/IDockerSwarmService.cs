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

    // Resource management methods
    Task UpdateServiceResourcesAsync(
        string serviceName,
        long? cpuLimitNanoCpus,
        long? cpuReservationNanoCpus,
        long? memoryLimitBytes,
        long? memoryReservationBytes,
        long? pidsLimit,
        List<UlimitDTO>? ulimits,
        CancellationToken cancellationToken = default);

    // Volume management methods
    Task<IList<VolumeResponse>> GetVolumesAsync(CancellationToken cancellationToken = default);
    Task<VolumeResponse?> GetVolumeByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<IList<VolumeResponse>> GetUnusedVolumesAsync(CancellationToken cancellationToken = default);
    Task<string> CreateVolumeAsync(CreateVolumeRequest request, CancellationToken cancellationToken = default);
    Task DeleteVolumeAsync(string volumeName, bool force = false, CancellationToken cancellationToken = default);
    Task<(int count, long spaceReclaimed, List<string> deletedVolumes)> PruneVolumesAsync(CancellationToken cancellationToken = default);
    Task<BackupVolumeResponse> BackupVolumeAsync(string volumeName, string destinationPath, CancellationToken cancellationToken = default);
    Task<RestoreVolumeResponse> RestoreVolumeAsync(string volumeName, string sourcePath, CancellationToken cancellationToken = default);
    Task<long> GetVolumeSizeAsync(string volumeName, CancellationToken cancellationToken = default);
    Task<IList<string>> GetContainersUsingVolumeAsync(string volumeName, CancellationToken cancellationToken = default);

    // Container management methods
    Task<IList<ContainerListResponse>> GetContainersAsync(bool all = true, CancellationToken cancellationToken = default);
    Task<ContainerInspectResponse?> GetContainerByIdAsync(string containerId, CancellationToken cancellationToken = default);
    Task<ContainerStatsDTO> GetContainerStatsAsync(string containerId, CancellationToken cancellationToken = default);
    Task<ContainerSizeDTO> GetContainerSizeAsync(string containerId, CancellationToken cancellationToken = default);
    Task<string> GetContainerLogsAsync(string containerId, int? tail = null, bool timestamps = false, CancellationToken cancellationToken = default);
    Task<ContainerExecResponse> ExecContainerAsync(string containerId, ContainerExecRequest request, CancellationToken cancellationToken = default);
    Task<ContainerTopDTO> GetContainerTopAsync(string containerId, CancellationToken cancellationToken = default);
    Task<IList<ContainerFileSystemChangeResponse>> GetContainerChangesAsync(string containerId, CancellationToken cancellationToken = default);

    // Metrics and monitoring methods
    Task<IList<ContainerMetricsSummaryDTO>> GetAllContainersMetricsSummaryAsync(CancellationToken cancellationToken = default);
    Task<IList<DockerEventDTO>> GetDockerEventsAsync(DateTime? since = null, DateTime? until = null, CancellationToken cancellationToken = default);

    // Network management methods
    Task<IList<NetworkResponse>> GetNetworksAsync(CancellationToken cancellationToken = default);
    Task<NetworkResponse?> GetNetworkByNameAsync(string name, CancellationToken cancellationToken = default);
    Task<string> CreateNetworkAsync(CreateNetworkRequest request, CancellationToken cancellationToken = default);
    Task DeleteNetworkAsync(string networkName, CancellationToken cancellationToken = default);
    Task<(int count, List<string> deletedNetworks)> PruneNetworksAsync(CancellationToken cancellationToken = default);
    Task ConnectContainerToNetworkAsync(string networkName, ConnectContainerRequest request, CancellationToken cancellationToken = default);
    Task DisconnectContainerFromNetworkAsync(string networkName, DisconnectContainerRequest request, CancellationToken cancellationToken = default);

    // Image management methods
    Task<IList<ImagesListResponse>> GetImagesAsync(bool all = false, CancellationToken cancellationToken = default);
    Task<ImageInspectResponse?> GetImageByIdAsync(string imageId, CancellationToken cancellationToken = default);
    Task<IList<ImageHistoryResponse>> GetImageHistoryAsync(string imageId, CancellationToken cancellationToken = default);
    Task<IList<ImagesListResponse>> GetDanglingImagesAsync(CancellationToken cancellationToken = default);
    Task<PullImageResponse> PullImageAsync(PullImageRequest request, CancellationToken cancellationToken = default);
    Task DeleteImageAsync(string imageId, bool force = false, bool pruneChildren = false, CancellationToken cancellationToken = default);
    Task<TagImageResponse> TagImageAsync(string imageId, TagImageRequest request, CancellationToken cancellationToken = default);
    Task<PushImageResponse> PushImageAsync(string imageId, PushImageRequest request, CancellationToken cancellationToken = default);
    Task<(int count, long spaceReclaimed, List<string> deletedImages)> PruneImagesAsync(bool dangling = true, CancellationToken cancellationToken = default);
    Task<int> GetImageContainerCountAsync(string imageId, CancellationToken cancellationToken = default);

    // Stack management methods
    Task<IList<StackDTO>> GetStacksAsync(CancellationToken cancellationToken = default);
    Task<StackDetailsDTO?> GetStackByNameAsync(string stackName, CancellationToken cancellationToken = default);
    Task<DeployStackResponse> DeployStackAsync(DeployStackRequest request, CancellationToken cancellationToken = default);
    Task DeleteStackAsync(string stackName, CancellationToken cancellationToken = default);
    Task<IList<StackServiceDTO>> GetStackServicesAsync(string stackName, CancellationToken cancellationToken = default);
}
