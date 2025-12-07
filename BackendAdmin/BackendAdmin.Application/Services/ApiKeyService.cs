using BackendAdmin.Application.Data;
using System.Security.Cryptography;

namespace BackendAdmin.Application.Services;

public interface IApiKeyService
{
    Task<ApiKeyResponse> CreateApiKeyAsync(CreateApiKeyRequest request);
    Task<ApiKeyValidationResult?> ValidateApiKeyAsync(string apiKey);
    Task RevokeApiKeyAsync(string apiKeyHash, string reason);
    Task<ApiKeyResponse> RotateApiKeyAsync(string currentApiKeyHash, int gracePeriodDays = 7);
    Task<IEnumerable<ApiKeyInfo>> GetApiKeysByApplicationAsync(string applicationId);
}

public record CreateApiKeyRequest
{
    public string ApplicationId { get; init; } = string.Empty;
    public string ApplicationName { get; init; } = string.Empty;
    public string[] Scopes { get; init; } = [];
    public DateTime? ExpiresAt { get; init; }
}

public record ApiKeyResponse
{
    public string ApiKey { get; init; } = string.Empty;
    public string ApiKeyHash { get; init; } = string.Empty;
    public string ApplicationId { get; init; } = string.Empty;
    public string ApplicationName { get; init; } = string.Empty;
    public string[] Scopes { get; init; } = [];
    public DateTime CreatedAt { get; init; }
    public DateTime? ExpiresAt { get; init; }
}

public record ApiKeyValidationResult
{
    public bool IsValid { get; init; }
    public string? ApiKeyHash { get; init; }
    public string? ApplicationId { get; init; }
    public string? ApplicationName { get; init; }
    public string[] Scopes { get; init; } = [];
    public string? ErrorMessage { get; init; }
}

public record ApiKeyInfo
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

public class ApiKeyService : IApiKeyService
{
    private readonly IApplicationDbContext _dbContext;
    private readonly IGenericRepository<ApiKey> _apiKeyRepository;
    private readonly IUnitOfWork _unitOfWork;

    public ApiKeyService(
        IApplicationDbContext dbContext,
        IGenericRepository<ApiKey> apiKeyRepository,
        IUnitOfWork unitOfWork)
    {
        _dbContext = dbContext;
        _apiKeyRepository = apiKeyRepository;
        _unitOfWork = unitOfWork;
    }

    public async Task<ApiKeyResponse> CreateApiKeyAsync(CreateApiKeyRequest request)
    {
        var rawApiKey = GenerateSecureApiKey();
        var apiKeyHash = HashApiKey(rawApiKey);

        var apiKey = new ApiKey
        {
            Id = Guid.NewGuid(),
            ApiKeyHash = apiKeyHash,
            ApplicationId = request.ApplicationId,
            ApplicationName = request.ApplicationName,
            Scopes = string.Join(",", request.Scopes),
            CreatedAt = DateTime.UtcNow,
            ExpiresAt = request.ExpiresAt ?? DateTime.UtcNow.AddDays(365),
            IsRevoked = false
        };

        await _apiKeyRepository.AddDataAsync(apiKey, CancellationToken.None);
        await _unitOfWork.SaveChangesDataAsync(CancellationToken.None);

        return new ApiKeyResponse
        {
            ApiKey = rawApiKey,
            ApiKeyHash = apiKeyHash,
            ApplicationId = apiKey.ApplicationId,
            ApplicationName = apiKey.ApplicationName,
            Scopes = request.Scopes,
            CreatedAt = apiKey.CreatedAt,
            ExpiresAt = apiKey.ExpiresAt
        };
    }

    public async Task<ApiKeyValidationResult?> ValidateApiKeyAsync(string apiKey)
    {
        var hash = HashApiKey(apiKey);

        var entity = await _dbContext.ApiKeys
            .FirstOrDefaultAsync(k => k.ApiKeyHash == hash);

        if (entity == null)
        {
            return new ApiKeyValidationResult
            {
                IsValid = false,
                ErrorMessage = "API Key invalide"
            };
        }

        if (entity.IsRevoked)
        {
            return new ApiKeyValidationResult
            {
                IsValid = false,
                ErrorMessage = "API Key revoquee"
            };
        }

        if (entity.ExpiresAt.HasValue && entity.ExpiresAt < DateTime.UtcNow)
        {
            return new ApiKeyValidationResult
            {
                IsValid = false,
                ErrorMessage = "API Key expiree"
            };
        }

        entity.LastUsedAt = DateTime.UtcNow;
        _apiKeyRepository.UpdateData(entity);
        await _unitOfWork.SaveChangesDataAsync(CancellationToken.None);

        return new ApiKeyValidationResult
        {
            IsValid = true,
            ApiKeyHash = entity.ApiKeyHash,
            ApplicationId = entity.ApplicationId,
            ApplicationName = entity.ApplicationName,
            Scopes = entity.Scopes.Split(',', StringSplitOptions.RemoveEmptyEntries)
        };
    }

    public async Task RevokeApiKeyAsync(string apiKeyHash, string reason)
    {
        var entity = await _dbContext.ApiKeys
            .FirstOrDefaultAsync(k => k.ApiKeyHash == apiKeyHash);

        if (entity == null)
        {
            throw new NotFoundException("ApiKey", apiKeyHash);
        }

        entity.IsRevoked = true;
        entity.RevokedReason = reason;
        entity.RevokedAt = DateTime.UtcNow;

        _apiKeyRepository.UpdateData(entity);
        await _unitOfWork.SaveChangesDataAsync(CancellationToken.None);
    }

    public async Task<ApiKeyResponse> RotateApiKeyAsync(string currentApiKeyHash, int gracePeriodDays = 7)
    {
        var currentEntity = await _dbContext.ApiKeys
            .FirstOrDefaultAsync(k => k.ApiKeyHash == currentApiKeyHash);

        if (currentEntity == null)
        {
            throw new NotFoundException("ApiKey", currentApiKeyHash);
        }

        var createRequest = new CreateApiKeyRequest
        {
            ApplicationId = currentEntity.ApplicationId,
            ApplicationName = currentEntity.ApplicationName,
            Scopes = currentEntity.Scopes.Split(',', StringSplitOptions.RemoveEmptyEntries),
            ExpiresAt = currentEntity.ExpiresAt
        };

        var newApiKey = await CreateApiKeyAsync(createRequest);

        if (gracePeriodDays <= 0)
        {
            currentEntity.IsRevoked = true;
            currentEntity.RevokedReason = "Rotation - revoquee immediatement";
            currentEntity.RevokedAt = DateTime.UtcNow;
        }
        else
        {
            currentEntity.ExpiresAt = DateTime.UtcNow.AddDays(gracePeriodDays);
            currentEntity.RevokedReason = $"Rotation - expire apres {gracePeriodDays} jours de grace";
        }

        _apiKeyRepository.UpdateData(currentEntity);
        await _unitOfWork.SaveChangesDataAsync(CancellationToken.None);

        return newApiKey;
    }

    public async Task<IEnumerable<ApiKeyInfo>> GetApiKeysByApplicationAsync(string applicationId)
    {
        var apiKeys = await _dbContext.ApiKeys
            .Where(k => k.ApplicationId == applicationId)
            .ToListAsync();

        return apiKeys.Select(k => new ApiKeyInfo
        {
            ApiKeyHash = k.ApiKeyHash,
            ApplicationId = k.ApplicationId,
            ApplicationName = k.ApplicationName,
            Scopes = k.Scopes.Split(',', StringSplitOptions.RemoveEmptyEntries),
            CreatedAt = k.CreatedAt,
            ExpiresAt = k.ExpiresAt,
            IsRevoked = k.IsRevoked,
            RevokedReason = k.RevokedReason,
            LastUsedAt = k.LastUsedAt
        });
    }

    private static string GenerateSecureApiKey()
    {
        var bytes = new byte[48];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(bytes);
        return Convert.ToBase64String(bytes);
    }

    private static string HashApiKey(string apiKey)
    {
        var bytes = System.Text.Encoding.UTF8.GetBytes(apiKey);
        var hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash).ToLowerInvariant();
    }
}
