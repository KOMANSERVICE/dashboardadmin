using FrontendAdmin.Shared.Pages.Swarm.Models;

namespace FrontendAdmin.Shared.Services.Https;

public interface ISwarmHttpService
{
    // Nodes
    [Get("/api/swarm/nodes")]
    Task<BaseResponse<GetNodesResponse>> GetNodesAsync();

    // Services - List
    [Get("/api/swarm/services")]
    Task<BaseResponse<GetSwarmServicesResponse>> GetServicesAsync();

    // Services - Details
    [Get("/api/swarm/services/{name}")]
    Task<BaseResponse<GetServiceDetailsResponse>> GetServiceDetailsAsync(string name);

    // Services - Tasks
    [Get("/api/swarm/services/{name}/tasks")]
    Task<BaseResponse<GetServiceTasksResponse>> GetServiceTasksAsync(string name);

    // Services - Logs
    [Get("/api/swarm/services/{name}/logs")]
    Task<BaseResponse<GetServiceLogsResponse>> GetServiceLogsAsync(string name, [Query] int? tail = null, [Query] string? since = null);

    // Services - Create
    [Post("/api/swarm/services")]
    Task<BaseResponse<CreateServiceResponse>> CreateServiceAsync([Body] CreateServiceRequest request);

    // Services - Update
    [Put("/api/swarm/services/{name}")]
    Task<BaseResponse<UpdateServiceResponse>> UpdateServiceAsync(string name, [Body] UpdateServiceRequest request);

    // Services - Delete
    [Delete("/api/swarm/services/{name}")]
    Task DeleteServiceAsync(string name);

    // Services - Scale
    [Post("/api/swarm/services/{name}/scale")]
    Task<BaseResponse<ScaleServiceResponse>> ScaleServiceAsync(string name, [Body] ScaleServiceRequest request);

    // Services - Restart
    [Post("/api/swarm/services/{name}/restart")]
    Task<BaseResponse<object>> RestartServiceAsync(string name);

    // Services - Rollback
    [Post("/api/swarm/services/{name}/rollback")]
    Task<BaseResponse<RollbackServiceResponse>> RollbackServiceAsync(string name);

    // Volumes - List
    [Get("/api/swarm/volumes")]
    Task<BaseResponse<GetVolumesResponse>> GetVolumesAsync();

    // Volumes - Details
    [Get("/api/swarm/volumes/{name}")]
    Task<BaseResponse<GetVolumeDetailsResponse>> GetVolumeDetailsAsync(string name);

    // Volumes - Unused
    [Get("/api/swarm/volumes/unused")]
    Task<BaseResponse<GetUnusedVolumesResponse>> GetUnusedVolumesAsync();

    // Volumes - Create
    [Post("/api/swarm/volumes")]
    Task<BaseResponse<CreateVolumeResponse>> CreateVolumeAsync([Body] CreateVolumeRequest request);

    // Volumes - Delete
    [Delete("/api/swarm/volumes/{name}")]
    Task DeleteVolumeAsync(string name, [Query] bool force = false);

    // Volumes - Prune
    [Post("/api/swarm/volumes/prune")]
    Task<BaseResponse<PruneVolumesResponse>> PruneVolumesAsync();

    // Volumes - Backup
    [Post("/api/swarm/volumes/{name}/backup")]
    Task<BaseResponse<BackupVolumeResponse>> BackupVolumeAsync(string name, [Body] BackupVolumeRequest request);

    // Volumes - Restore
    [Post("/api/swarm/volumes/{name}/restore")]
    Task<BaseResponse<RestoreVolumeResponse>> RestoreVolumeAsync(string name, [Body] RestoreVolumeRequest request);

    // Containers - List
    [Get("/api/swarm/containers")]
    Task<BaseResponse<GetContainersResponse>> GetContainersAsync([Query] bool? all = true);

    // Containers - Stats
    [Get("/api/swarm/containers/{id}/stats")]
    Task<BaseResponse<GetContainerStatsResponse>> GetContainerStatsAsync(string id);

    // Containers - Size
    [Get("/api/swarm/containers/{id}/size")]
    Task<BaseResponse<GetContainerSizeResponse>> GetContainerSizeAsync(string id);

    // Containers - Logs
    [Get("/api/swarm/containers/{id}/logs")]
    Task<BaseResponse<GetContainerLogsResponse>> GetContainerLogsAsync(string id, [Query] int? tail = null, [Query] bool? timestamps = false);

    // Containers - Exec
    [Post("/api/swarm/containers/{id}/exec")]
    Task<BaseResponse<ExecContainerResponse>> ExecContainerAsync(string id, [Body] ContainerExecRequest request);

    // Containers - Inspect
    [Get("/api/swarm/containers/{id}/inspect")]
    Task<BaseResponse<GetContainerInspectResponse>> GetContainerInspectAsync(string id);

    // Containers - Top (processes)
    [Get("/api/swarm/containers/{id}/top")]
    Task<BaseResponse<GetContainerTopResponse>> GetContainerTopAsync(string id);

    // Containers - Changes
    [Get("/api/swarm/containers/{id}/changes")]
    Task<BaseResponse<GetContainerChangesResponse>> GetContainerChangesAsync(string id);

    // Containers - Metrics Summary (all containers)
    [Get("/api/swarm/containers/metrics")]
    Task<BaseResponse<GetContainersMetricsSummaryResponse>> GetContainersMetricsSummaryAsync();

    // Docker Events
    [Get("/api/swarm/events")]
    Task<BaseResponse<GetDockerEventsResponse>> GetDockerEventsAsync([Query] DateTime? since = null, [Query] DateTime? until = null);

    // Services - Resources
    [Get("/api/swarm/services/{name}/resources")]
    Task<BaseResponse<GetServiceResourcesResponse>> GetServiceResourcesAsync(string name);

    // Services - Update Resources
    [Put("/api/swarm/services/{name}/resources")]
    Task<BaseResponse<UpdateServiceResourcesResponse>> UpdateServiceResourcesAsync(string name, [Body] UpdateServiceResourcesRequest request);

    // Networks - List
    [Get("/api/swarm/networks")]
    Task<BaseResponse<GetNetworksResponse>> GetNetworksAsync();

    // Networks - Details
    [Get("/api/swarm/networks/{name}")]
    Task<BaseResponse<GetNetworkDetailsResponse>> GetNetworkDetailsAsync(string name);

    // Networks - Create
    [Post("/api/swarm/networks")]
    Task<BaseResponse<CreateNetworkResponse>> CreateNetworkAsync([Body] CreateNetworkRequest request);

    // Networks - Delete
    [Delete("/api/swarm/networks/{name}")]
    Task DeleteNetworkAsync(string name);

    // Networks - Prune
    [Post("/api/swarm/networks/prune")]
    Task<BaseResponse<PruneNetworksResponse>> PruneNetworksAsync();

    // Networks - Connect Container
    [Post("/api/swarm/networks/{name}/connect")]
    Task<BaseResponse<ConnectContainerResponse>> ConnectContainerAsync(string name, [Body] ConnectContainerRequest request);

    // Networks - Disconnect Container
    [Post("/api/swarm/networks/{name}/disconnect")]
    Task<BaseResponse<DisconnectContainerResponse>> DisconnectContainerAsync(string name, [Body] DisconnectContainerRequest request);

    // Images - List
    [Get("/api/swarm/images")]
    Task<BaseResponse<GetImagesResponse>> GetImagesAsync([Query] bool? all = false);

    // Images - Details
    [Get("/api/swarm/images/{id}")]
    Task<BaseResponse<GetImageDetailsResponse>> GetImageDetailsAsync(string id);

    // Images - History
    [Get("/api/swarm/images/{id}/history")]
    Task<BaseResponse<GetImageHistoryResponse>> GetImageHistoryAsync(string id);

    // Images - Dangling
    [Get("/api/swarm/images/dangling")]
    Task<BaseResponse<GetDanglingImagesResponse>> GetDanglingImagesAsync();

    // Images - Pull
    [Post("/api/swarm/images/pull")]
    Task<BaseResponse<PullImageResponse>> PullImageAsync([Body] PullImageRequest request);

    // Images - Delete
    [Delete("/api/swarm/images/{id}")]
    Task DeleteImageAsync(string id, [Query] bool force = false, [Query] bool pruneChildren = false);

    // Images - Tag
    [Post("/api/swarm/images/{id}/tag")]
    Task<BaseResponse<TagImageResponse>> TagImageAsync(string id, [Body] TagImageRequest request);

    // Images - Push
    [Post("/api/swarm/images/{id}/push")]
    Task<BaseResponse<PushImageResponse>> PushImageAsync(string id, [Body] PushImageRequest request);

    // Images - Prune
    [Post("/api/swarm/images/prune")]
    Task<BaseResponse<PruneImagesResponse>> PruneImagesAsync([Query] bool? dangling = true);

    // Stacks - List
    [Get("/api/swarm/stacks")]
    Task<BaseResponse<GetStacksResponse>> GetStacksAsync();

    // Stacks - Details
    [Get("/api/swarm/stacks/{name}")]
    Task<BaseResponse<GetStackDetailsResponse>> GetStackDetailsAsync(string name);

    // Stacks - Services
    [Get("/api/swarm/stacks/{name}/services")]
    Task<BaseResponse<GetStackServicesResponse>> GetStackServicesAsync(string name);

    // Stacks - Deploy
    [Post("/api/swarm/stacks")]
    Task<BaseResponse<DeployStackResponse>> DeployStackAsync([Body] DeployStackRequest request);

    // Stacks - Delete
    [Delete("/api/swarm/stacks/{name}")]
    Task DeleteStackAsync(string name);

    // System - Info
    [Get("/api/swarm/system/info")]
    Task<BaseResponse<GetSystemInfoResponse>> GetSystemInfoAsync();

    // System - Version
    [Get("/api/swarm/system/version")]
    Task<BaseResponse<GetDockerVersionResponse>> GetDockerVersionAsync();

    // System - Disk Usage
    [Get("/api/swarm/system/disk")]
    Task<BaseResponse<GetDiskUsageResponse>> GetDiskUsageAsync();

    // System - Prune All
    [Post("/api/swarm/system/prune")]
    Task<BaseResponse<PruneAllResponse>> PruneAllAsync();
}
