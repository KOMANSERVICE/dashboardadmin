using FrontendAdmin.Shared.Pages.Swarm.Models;

namespace FrontendAdmin.Shared.Services.Https;

public interface ISwarmHttpService
{
    // Nodes
    [Get("/swarm/nodes")]
    Task<BaseResponse<GetNodesResponse>> GetNodesAsync();

    // Services - List
    [Get("/swarm/services")]
    Task<BaseResponse<GetSwarmServicesResponse>> GetServicesAsync();

    // Services - Details
    [Get("/swarm/services/{name}")]
    Task<BaseResponse<GetServiceDetailsResponse>> GetServiceDetailsAsync(string name);

    // Services - Tasks
    [Get("/swarm/services/{name}/tasks")]
    Task<BaseResponse<GetServiceTasksResponse>> GetServiceTasksAsync(string name);

    // Services - Logs
    [Get("/swarm/services/{name}/logs")]
    Task<BaseResponse<GetServiceLogsResponse>> GetServiceLogsAsync(string name, [Query] int? tail = null, [Query] string? since = null);

    // Services - Create
    [Post("/swarm/services")]
    Task<BaseResponse<CreateServiceResponse>> CreateServiceAsync([Body] CreateServiceRequest request);

    // Services - Update
    [Put("/swarm/services/{name}")]
    Task<BaseResponse<UpdateServiceResponse>> UpdateServiceAsync(string name, [Body] UpdateServiceRequest request);

    // Services - Delete
    [Delete("/swarm/services/{name}")]
    Task DeleteServiceAsync(string name);

    // Services - Scale
    [Post("/swarm/services/{name}/scale")]
    Task<BaseResponse<ScaleServiceResponse>> ScaleServiceAsync(string name, [Body] ScaleServiceRequest request);

    // Services - Restart
    [Post("/swarm/services/{name}/restart")]
    Task<BaseResponse<object>> RestartServiceAsync(string name);

    // Services - Rollback
    [Post("/swarm/services/{name}/rollback")]
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
}
