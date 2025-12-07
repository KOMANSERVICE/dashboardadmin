using FrontendAdmin.Shared.Models;
using FrontendAdmin.Shared.Pages.ApiKeys.Models;

namespace FrontendAdmin.Shared.Services.Https;

public interface IApiKeyHttpService
{
    /// <summary>
    /// Create a new API Key for an application
    /// </summary>
    [Post("/apikeys")]
    Task<BaseResponse<ApiKeyCreatedResponse>> CreateApiKeyAsync(CreateApiKeyRequest request);

    /// <summary>
    /// Get all API Keys for an application
    /// </summary>
    [Get("/apikeys/{applicationId}")]
    Task<BaseResponse<GetApiKeysResponse>> GetApiKeysByApplicationAsync(string applicationId);

    /// <summary>
    /// Revoke an API Key
    /// </summary>
    [Delete("/apikeys")]
    Task<BaseResponse<bool>> RevokeApiKeyAsync([Body] RevokeApiKeyRequest request);

    /// <summary>
    /// Rotate an API Key with grace period
    /// </summary>
    [Post("/apikeys/rotate")]
    Task<BaseResponse<ApiKeyCreatedResponse>> RotateApiKeyAsync(RotateApiKeyRequest request);
}
