using BackendAdmin.Application.Features.ApiKeys.DTOs;
using BackendAdmin.Application.Services;

namespace BackendAdmin.Application.Features.ApiKeys.Queries.GetApiKeysByApplication;

public class GetApiKeysByApplicationHandler(
    IApiKeyService _apiKeyService
) : IQueryHandler<GetApiKeysByApplicationQuery, GetApiKeysByApplicationResult>
{
    public async Task<GetApiKeysByApplicationResult> Handle(
        GetApiKeysByApplicationQuery request,
        CancellationToken cancellationToken)
    {
        var apiKeys = await _apiKeyService.GetApiKeysByApplicationAsync(request.ApplicationId);

        var apiKeyDtos = apiKeys.Select(k => new ApiKeyInfoDTO
        {
            ApiKeyHash = k.ApiKeyHash,
            ApplicationId = k.ApplicationId,
            ApplicationName = k.ApplicationName,
            Scopes = k.Scopes,
            CreatedAt = k.CreatedAt,
            ExpiresAt = k.ExpiresAt,
            IsRevoked = k.IsRevoked,
            RevokedReason = k.RevokedReason,
            LastUsedAt = k.LastUsedAt
        });

        return new GetApiKeysByApplicationResult(apiKeyDtos);
    }
}
