namespace BackendAdmin.Application.Features.ApiKeys.DTOs;

public record CreateApiKeyDTO
{
    public string ApplicationId { get; init; } = string.Empty;
    public string ApplicationName { get; init; } = string.Empty;
    public string[] Scopes { get; init; } = [];
    public DateTime? ExpiresAt { get; init; }
}

public record ApiKeyInfoDTO
{
    public string ApiKeyHash { get; init; } = string.Empty;
    public string ApplicationId { get; init; } = string.Empty;
    public string ApplicationName { get; init; } = string.Empty;
    public string[] Scopes { get; init; } = [];
    public DateTime CreatedAt { get; init; }
    public DateTime? ExpiresAt { get; init; }
    public bool IsRevoked { get; init; }
    public string? RevokedReason { get; init; }
    public DateTime? LastUsedAt { get; init; }
}

public record ApiKeyCreatedDTO
{
    public string ApiKey { get; init; } = string.Empty;
    public string ApiKeyHash { get; init; } = string.Empty;
    public string ApplicationId { get; init; } = string.Empty;
    public string ApplicationName { get; init; } = string.Empty;
    public string[] Scopes { get; init; } = [];
    public DateTime CreatedAt { get; init; }
    public DateTime? ExpiresAt { get; init; }
}

public record RevokeApiKeyDTO
{
    public string ApiKeyHash { get; init; } = string.Empty;
    public string Reason { get; init; } = string.Empty;
}

public record RotateApiKeyDTO
{
    public string CurrentApiKeyHash { get; init; } = string.Empty;
    public int GracePeriodDays { get; init; } = 7;
}
