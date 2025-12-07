using BackendAdmin.Application.Features.ApiKeys.DTOs;
using BackendAdmin.Application.Services;

namespace BackendAdmin.Application.Features.ApiKeys.Commands.CreateApiKey;

public class CreateApiKeyHandler(
    IApiKeyService _apiKeyService
) : ICommandHandler<CreateApiKeyCommand, CreateApiKeyResult>
{
    public async Task<CreateApiKeyResult> Handle(CreateApiKeyCommand request, CancellationToken cancellationToken)
    {
        var dto = request.ApiKey;

        var createRequest = new CreateApiKeyRequest
        {
            ApplicationId = dto.ApplicationId,
            ApplicationName = dto.ApplicationName,
            Scopes = dto.Scopes,
            ExpiresAt = dto.ExpiresAt ?? DateTime.UtcNow.AddDays(365)
        };

        var response = await _apiKeyService.CreateApiKeyAsync(createRequest);

        var resultDto = new ApiKeyCreatedDTO
        {
            ApiKey = response.ApiKey,
            ApiKeyHash = response.ApiKeyHash,
            ApplicationId = response.ApplicationId,
            ApplicationName = response.ApplicationName,
            Scopes = response.Scopes,
            CreatedAt = response.CreatedAt,
            ExpiresAt = response.ExpiresAt
        };

        return new CreateApiKeyResult(resultDto);
    }
}
