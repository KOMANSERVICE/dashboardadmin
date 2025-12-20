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
}
