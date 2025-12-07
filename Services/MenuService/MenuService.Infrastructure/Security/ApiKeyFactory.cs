using IDR.Library.BuildingBlocks.Abstractions;
using IDR.Library.BuildingBlocks.Security.Interfaces;
using MenuService.Domain.Models;

namespace MenuService.Infrastructure.Security;

/// <summary>
/// Factory implementation for creating ApiKey instances.
/// Used by IApiKeyService from IDR.Library.BuildingBlocks.
/// </summary>
public class ApiKeyFactory : IApiKeyFactory
{
    public IApiKey CreateApiKey(
        string apiKeyHash,
        string applicationId,
        string applicationName,
        string scopes,
        DateTime? expiresAt)
    {
        return new ApiKey
        {
            Id = Guid.NewGuid(),
            ApiKeyHash = apiKeyHash,
            ApplicationId = applicationId,
            ApplicationName = applicationName,
            Scopes = scopes,
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = expiresAt,
            IsRevoked = false,
            RevokedReason = null,
            RevokedAt = null,
            LastUsedAt = null
        };
    }
}
