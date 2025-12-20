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
    [Get("/swarm/volumes")]
    Task<BaseResponse<GetVolumesResponse>> GetVolumesAsync();

    // Volumes - Details
    [Get("/swarm/volumes/{name}")]
    Task<BaseResponse<GetVolumeDetailsResponse>> GetVolumeDetailsAsync(string name);

    // Volumes - Unused
    [Get("/swarm/volumes/unused")]
    Task<BaseResponse<GetUnusedVolumesResponse>> GetUnusedVolumesAsync();

    // Volumes - Create
    [Post("/swarm/volumes")]
    Task<BaseResponse<CreateVolumeResponse>> CreateVolumeAsync([Body] CreateVolumeRequest request);

    // Volumes - Delete
    [Delete("/swarm/volumes/{name}")]
    Task DeleteVolumeAsync(string name, [Query] bool force = false);

    // Volumes - Prune
    [Post("/swarm/volumes/prune")]
    Task<BaseResponse<PruneVolumesResponse>> PruneVolumesAsync();

    // Volumes - Backup
    [Post("/swarm/volumes/{name}/backup")]
    Task<BaseResponse<BackupVolumeResponse>> BackupVolumeAsync(string name, [Body] BackupVolumeRequest request);

    // Volumes - Restore
    [Post("/swarm/volumes/{name}/restore")]
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

    // Services - Resources
    [Get("/api/swarm/services/{name}/resources")]
    Task<BaseResponse<GetServiceResourcesResponse>> GetServiceResourcesAsync(string name);

    // Services - Update Resources
    [Put("/api/swarm/services/{name}/resources")]
    Task<BaseResponse<UpdateServiceResourcesResponse>> UpdateServiceResourcesAsync(string name, [Body] UpdateServiceResourcesRequest request);
}
